# SeasonDemo

SeasonDemo is a small Unity 6 sample project with EditMode tests and a GitHub Actions workflow for cloud builds.

## Unity Version

- Unity `6000.3.18f1`
- Project path layout: `Assets/`, `Packages/`, `ProjectSettings/`

## Cloud Build

This repository is prepared for open-source cloud builds with GitHub Actions and GameCI.

Required GitHub repository secrets:

- `UNITY_LICENSE`: Personal license file contents from `Unity_lic.ulf`.
- `UNITY_SERIAL`: Plus/Pro serial key. Use this instead of `UNITY_LICENSE` for paid seats.
- `UNITY_EMAIL`: Unity account email.
- `UNITY_PASSWORD`: Unity account password.

For a Personal license, Unity normally writes the license file here on macOS:

```bash
/Library/Application\ Support/Unity/Unity_lic.ulf
```

If that file does not exist, open Unity Hub, go to `Preferences > Licenses`, click `Add`, and activate a free Personal license.

The workflow lives at `.github/workflows/unity-ci.yml` and runs:

1. EditMode tests with `game-ci/unity-test-runner`.
2. An Android APK build with `game-ci/unity-builder`.
3. Test result and build artifact uploads.

The Android build uses `SeasonDemoEditor.SeasonDemoProjectConfigurator.BuildAndroid` and uploads `Builds/SeasonDemo.apk`.

To add more platforms, extend the `targetPlatform` matrix in `.github/workflows/unity-ci.yml`. Release-signed Android builds and iOS builds usually need extra signing secrets before they can publish store artifacts.

## Local Test Command

On macOS with this Unity version installed:

```bash
mkdir -p TestResults Logs

"/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -nographics \
  -projectPath "$(pwd)" \
  -runTests \
  -testPlatform EditMode \
  -testResults "$(pwd)/TestResults/EditMode.xml" \
  -logFile "$(pwd)/Logs/editmode-tests.log" \
  -quit
```

## Local Android Build Command

```bash
"/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -projectPath "$(pwd)" \
  -executeMethod SeasonDemoEditor.SeasonDemoProjectConfigurator.BuildAndroid \
  -logFile - \
  -quit
```

## Open Source Notes

- Commit `Assets/`, `Packages/`, `ProjectSettings/`, `.github/`, `.gitattributes`, `.gitignore`, and this README.
- Do not commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, build outputs, or generated IDE files.
- Choose and add a license file before publishing publicly.
