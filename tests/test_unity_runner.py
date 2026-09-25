import contextlib
import io
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch

from scripts import run_unity_tests as runner
from scripts import unity_common as unity


def xml(total=1, passed=1, failed=0, skipped=0, result='Passed', case_result='Passed'):
    case = f'<test-case fullname="CommonHouse.Tests.Probe" result="{case_result}"/>' if total else ''
    return f'<test-run total="{total}" passed="{passed}" failed="{failed}" skipped="{skipped}" inconclusive="0" result="{result}">{case}</test-run>'


class UnityRunnerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.base = Path(self.temp.name)
        self.results = self.base / 'results.xml'

    def test_matching_pass_is_required(self):
        self.results.write_text(xml())
        self.assertEqual(runner.validate_results(self.results)['passed'], 1)
        self.assertEqual(runner.validate_results(self.results, 'CommonHouse.Tests')['passed'], 1)
        with self.assertRaisesRegex(ValueError, 'outside the requested'):
            runner.validate_results(self.results, 'CommonHouse.Tests.OtherFixture')

    def test_missing_malformed_zero_failed_skipped_and_inconsistent_results_fail(self):
        with self.assertRaises(ValueError):
            runner.validate_results(self.results)
        for content in ['<broken', '<test-run/>', xml(total=0, passed=0),
                        xml(passed=0, failed=1, result='Failed', case_result='Failed'),
                        xml(passed=0, skipped=1), xml(case_result='Failed'),
                        xml(total=2, passed=2)]:
            self.results.write_text(content)
            with self.subTest(content=content), self.assertRaises(ValueError):
                runner.validate_results(self.results)

    def test_command_uses_arguments_without_quit_and_optional_graphics(self):
        command = runner.command('/Editor With Spaces/Unity', self.base / 'project', self.base, 'playmode', 'CommonHouse.Tests.Probe', False)
        self.assertEqual(command[0], '/Editor With Spaces/Unity')
        self.assertNotIn('-quit', command)
        self.assertIn('-nographics', command)
        self.assertNotIn('-nographics', runner.command('Unity', self.base, self.base, 'editmode', 'Probe', True))

    def test_timeout_terminates_owned_process_group(self):
        with patch.object(unity.subprocess, 'Popen') as popen, patch.object(unity.os, 'killpg') as kill:
            process = popen.return_value
            process.pid = 123456
            process.wait.side_effect = [subprocess.TimeoutExpired('Unity', 1), -9]
            self.assertEqual(unity.execute(['Unity'], self.base, 1, 'test'), 124)
            kill.assert_called_once_with(123456, unity.signal.SIGKILL)
            self.assertTrue(popen.call_args.kwargs['start_new_session'])

    def test_runtime_environment_omits_unrelated_secrets_and_uses_test_data(self):
        with patch.dict(unity.os.environ, {'EXAMPLE_PRODUCTION_TOKEN': 'do-not-pass'}):
            env = unity.development_env(self.base, 'test')
        self.assertNotIn('EXAMPLE_PRODUCTION_TOKEN', env)
        self.assertEqual(env['COMMON_HOUSE_ENV'], 'test')
        self.assertTrue(Path(env['COMMON_HOUSE_TEST_DATA_PATH']).is_dir())

    def test_main_failure_cleans_snapshot_and_save_but_keeps_evidence(self):
        project = self.base / 'project'
        save = self.base / 'save'
        def snapshot(*args):
            project.mkdir(); save.mkdir(); return {}
        with patch.object(unity, 'new_evidence', return_value=self.base), \
             patch.object(unity, 'resolve_editor', return_value=(Path('/Unity'), '6000.4.10f1')), \
             patch.object(unity, 'snapshot', side_effect=snapshot), \
             patch.object(unity, 'record_source'), \
             patch.object(unity, 'isolate_save_identity', return_value=save), \
             patch.object(unity, 'execute', return_value=198), \
             contextlib.redirect_stderr(io.StringIO()):
            self.assertEqual(runner.main(['--platform', 'editmode', '--filter', 'Probe']), 1)
        self.assertFalse(project.exists())
        self.assertFalse(save.exists())
        self.assertIn('198', (self.base / 'result.json').read_text())

    def test_missing_editor_is_failure_with_receipt(self):
        with patch.object(unity, 'new_evidence', return_value=self.base), \
             patch.object(unity, 'resolve_editor', side_effect=ValueError('Editor unavailable')), \
             contextlib.redirect_stderr(io.StringIO()):
            self.assertEqual(runner.main(['--platform', 'editmode', '--filter', 'Probe']), 1)
        self.assertIn('Editor unavailable', (self.base / 'result.json').read_text())

    def test_invalid_filter_is_rejected_before_execution(self):
        with patch.object(unity, 'execute') as execute, contextlib.redirect_stderr(io.StringIO()):
            with self.assertRaises(SystemExit):
                runner.main(['--platform', 'editmode', '--filter', 'Probe; command'])
            execute.assert_not_called()


if __name__ == '__main__':
    unittest.main()
