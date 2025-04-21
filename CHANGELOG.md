# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) and this project adheres to [Semantic Versioning](http://semver.org/).

Please see [here](https://github.com/hrntsm/Tunny/releases) for the data released for each version.

## [UNRELEASED] -YYYY-MM-DD

### Added

- Disable Rhino viewport updates during optimization checkbox in Settings/Misc tab.
  - The drawing to viewport stops, allowing faster optimization to be performed.
- Creating a file named quit.fishing in the .tunny_env folder to stop optimization.
- Optuna pruner support.
- Fully NEW UI
- Variables can now be set to log scale from the UI.
- Checkbox which minimize Rhino Window when start optimization
- Support some optunahub sampler.
  - Auto sampler, MOEA/D sampler, MO-CMA-ES sampler, DifferentialEvolution sampler, c-TPE sampler, HEBO sampler
- ConstructFishEggByCsv component
  - Make it easy to set FishEgg to use csv.
- Other Grasshopper component compatibility check.
- Support NetCore build
  - net7 for Rhino8
  - net9 for Rhino9
- Support mac Rhino8 and Rhino9
  - Both arm64 and x64
  - using eto UI
- Objective input values check
  - if null or list or tree input to objectives, Tunny throw error.
- Added error handling when an unexpected file path is entered in FishPrintByPath
  - support only bitmap type file.
- Tunny environment directory can open from Tunny fishing component menu items.
- Enhance GA base algorithm
  - Add mutation & crossover settings

### Changed

- Default log file is set to "Verbose" with a retention period of 1 day.
- Dashboard & TT-DesignExplorer trial attribute handling.
  - Attribute values are shown in ParallelCoordinate plot in dashboard.
  - Attribute values are shown in graph & trial value table in TT-DesignExplorer.
- Stop log output to console.
- Disable window minimize button.
- No log output to the Rhino console.
- Human in the loop code was moved to the Optuna project to improve maintainability.
- Remove Geometry & Direction input from ConstructFishAttribute component.
  - Study direction can set in new Tunny UI.
  - If you need save geometry, Please use artifact feature.
- Update Grasshopper UI enable/disable timing.
  - Disable when Tunny UI wake up
  - Enable when Tunny UI closed
- For improve visibility, ConstructFishAttribute is colored.
- Improve result log file load speed.
  - Tunny was loading the result file when launching the UI, and when the log file was large, it took a long time to launch.
  - With this update, log files are now about 60 times faster to load.
    - What used to take about 5 minutes to load the results of 30,000 trials can now be loaded in about 5 seconds.
- Add Fish panel output to artifact info.
- Update ConstructFishEgg component
  - Add fill color
  - Add SkipIfExit input
- Update optuna v4.3

### Removed

- Geometry & Direction input in ConstructFisAttr component.
  - If you use Geometry, please use artifact.
  - Direction can set in New Tunny UI.

### Fixed

- Tunny UI wont wake up when rhino7(net4.8) version settings file deserialize.
- FishPrintByPath locks the image.
- Mesh could not be put directly into Artifact.
- Human in the loop exception with stop_flag.
- Even the multi-purpose Human in the loop specifies GP and the optimization does not flow, so TPE is chosen.
- Pareto Front plot with 3 objectives visualize error.
- Delegate initialize error when push run optimize button.
- If there is not examples dir in .tunny_env, tunny throw exception when Loading instruction.
- Use try-catch syntax to avoid LoadingInstruction exception.
- Same capture is saved when using FishPrintByCapture.
- SQLite library conflict to speckle

### Security

in case of vulnerabilities.

## [v0.12.0] -2024-06-22

### Added

- Help & Install Python menu strip items
- The initial value of FishEgg be the mean value X0 assumed for the first normal distribution of CMA-ES.
- FishAttr now allows you to specify the optimization direction for each objective function.
  - A value of 1 indicates maximization, while -1 indicates minimization.
- BruteForceSampler
  - It is a sampler of the total number of hits.
- Output result for TT-Design Explorer
- Optimization state output to fishing component
- Use component OBSOLETE feature
  - Old components are now marked "OLD" to indicate that they are older version.
- TPE gamma setting.
  - The smaller of the number input and 10% of the completed trials is the number of trials included in L(x).
  - The higher this number, the higher the exploitation of the completed trials.

### Changed

- When outputting results from TunnyUI, even if the number of objective functions in Grasshopper does not match the number of objective functions in the result file, the results can still be output.
- After optimization is finished, a window allows the user to choose whether to reinstate the results or not.
- The words "reflect" and "restore" are changed to "reinstate" to match the Galapagos expression.
- ModelNumber in the output section is changed to TrialNumber.

### Fixed

- Python install error when there is optuna dashboard process
  - Add check and kill the process method
- Rhino7 and Rhino8 compatibility
  - There was a difference in specifications between Rhino 7 (net48) and Rhino 8 (net7) when serializing its Version to JSON for saving settings.
- Optuna-Dashboard doesn't work when artifact-dir contains space.
- Fixed an error when there is no file in the path specified in FishPrintByPath.
- Rhino crashes when reinstating a value to a slider if the categorical value is a number.
- MessageBox is now not below the back of Grasshopper window.

### Security

- Bump scikit-learn 1.3.1 => 1.5.0

## [v0.11.1] -2024-05-10

### Added

- Ignore duplicate sampling setting

### Changed

- Bump optuna library
  - optuna 3.6.0 => 3.6.1
  - optuna-dashboard 0.15.0 => 0.15.1

### Fixed

- SolutionExpire timing was wrong when using ValueList.
- FishAttribute component "Attr" input index off by one error.

## [v0.11.0] -2024-03-20

### Added

- Bone Fish component
  - It is Tunny UI less mode component.
- Categorical optimization.
  - Sample gh file also added
- Log output.
  - Improved ease of support.
- Log level selector.
  - It is located in the Misc tab of the Settings tab.
  - There are three levels: "Verbose," "Debug," and "Information.
- New GP sampler support
  - This is related optuna 3.6
- Sample python code to create a gif animation.
- Tunny.Core csproj for improve develop environment.
- CI build with python.

### Changed

- DeconstructFish component output
  - To support categorical optimization, the variable output was split from "Variables" into "NumberVariables" and "TextVariables".
- Move FishPrint component to Print subcategory
- Move Construct & Deconstruct component ot Operation subcategory
- Use optuna.csproj sqlite handler instead of tunny.storage
- Improve result fish output to use new storage handler
  - The handler is independent from python process.
- Human in the loop mode support log storage format.
- Allows clustering with any combination of variables and objective function.
- Speedup optimization.
  - Do not recalculate irrelevant components.
  - Previously, all components were recalculated for each trial, but this has been changed so that components not involved in optimization are not recalculated
- Bump Python library
  - optuna-dashboard 0.14 to 0.15
    - csv can be downloaded from the trial table.
    - The Pareto Front and History plots can jump to the corresponding trial page when click dot plot.
    - Settings have been added to change the appearance of the plots.
    - When uploading surfaces with no thickness to Artifacts, both sides are rendered.
    - Human-in-the-loop works with journal storage
    - Fix overflow when preferential optimization
    - [see more detail](https://github.com/optuna/optuna-dashboard/releases/tag/v0.15.0)
  - optuna 3.5 to 3.6
    - New GP sampler support
    - Fixed bug in parallelization in log files
    - [see more detail](https://github.com/optuna/optuna/releases/tag/v3.6.0)

### Fixed

- When launching a window, if all the objectives are null, the window will not launch.
- Empty 3dm is always saved even if you want to save only images in artifact.
- FishEgg feature does not work.
- Fish Print by capture timing
  - Viewport capture at the end of all Grasshopper processing.

## [v0.10.0] -2024-01-27

### Changed

- Use new JournalStorage handler to improve stability.
  - old format journal storage result new can open in tunny.

### Fixed

- Does not work when human-in-the-loop continues.
- Error when reading older versions of JournalStorage.
- without constraint notification timing when output result.
- in-memory optimization result saving error.

## [v0.9.1] -2023-12-30

### Fixed

- Error generated due to missing path in settings on first startup.

## [v0.9.0] -2023-12-30

### Added

- The seed value can be specified in BayesianOptimization(GP).
- Support optuna artifact feature.
  - GeometryBase & FishPrint & file path are supported.
- Allows Trial to FAIL.
  - In contrast to the constraint, if the value of the objective function cannot be obtained properly due to divergence of the solution or other reasons, it can be reflected in the trial.
  - It works by inputting a bool value in the Attributes with the key "IsFAIL".
- Preferential optimization support.
  - Human-in-the-loop optimization with PreferentialOptimization for single-objective images, and human-in-the-loop optimization with sliders for multi-objective images.

### Changed

- Tunny Python runtime environment location to [UserProfile]/.tunny_env
- Moved settings file location to [UserProfile]/.tunny_env
- Storage's default path to the desktop.
  - Previously, it was in the same folder as the Tunny component .gha file.
- Default StudyName set to empty.
  - If you don't input a name in Tunny UI, study Name using GUID will be automatically inserted.

### Fixed

- The seed value of the sampler was not used.
- Failure to launch Optuna-Dashboard from tab.

## [v0.8.2] -2023-09-04

### Changed

- Stopped putting built files together in gha.
  - Because some people did not work in some environments
- If the objective function contains null for 10 consecutive times, optimization is stopped.
- Update python lib
  - bump up optuna v3.3 & optuna-dashboard v0.12.0 & some library

### Fixed

- NSGA-III supports constraints, but was getting a message that constraints are not taken into account

## [v0.8.1] -2023-07-30

### Added

- Debug log output button

### Changed

- Improved stability of python library installation
  - The library is now included from the beginning in whl format and the file is installed, eliminating the need to download it from the Internet.

## [v0.8.0] -2023-07-24

### Added

- Support Human-in-the-loop optimization
  - Input FishPrint into the objective to start it
  - Add 2 sample gh file
- Support CMA-ES with Margin
  - It allows for more efficient optimization in mixed integer problems.
- Support NSGA-III
  - For more than 3 objective optimization.
- Python sample code
- Open optuna dashboard menu item

### Changed

- When optimizing with CMA-ES, the with Margin option is enabled by default.
- Support multi constraints.
- Bump up optuna v3.2.0
- Bump up optuna-dashboard v0.10.2

### Removed

- Grid sampling.

### Fixed

- Error occurs when remaining time becomes negative
- GC behavior settings configured in the UI during optimization are not reflected in optimization

### Security

- Bump up scipy v1.10.0

## [v0.7.2] -2023-03-22

### Added

- Support Rhino PackageManager.
- Show Tunny icon in grasshopper ribbon.

## [v0.7.1] -2023-03-22

### Fixed

- bug where visualize and output did not work when using journal storage.

## [v0.7.0] -2023-03-21

### Added

- Display of estimated remaining time during optimization run.
- In-memory optimization mode.
  - Mode that works faster instead of saving optimization results during optimization.
- Support Journal storage.
  - Since saving to the sqlite storage format that had been used up to now sometimes resulted in errors during optimization, a different storage format was supported.
- Checkbox to toggle whether results are shown in the UI in Realtime.
  - There was a problem that the display of results on the UI in Realtime, which was added in v0.6, caused the optimization speed to gradually slow down.

### Changed

- Boolean to start only the first time, since the Python installer may start every time.
  - If you want to install it again, you can do so by checking the checkbox from Misc in the Settings tab.
- The most of the dll files are combined into a single gha file to improve usability.

### Fixed

- Enabled Optuna-Dashboard to work even if the filename contains spaces.
- The problem of saving the results of optimization in progress, etc., which causes an error and fails to save the results, can now be avoided by using JournalStorage.

### Security

- Bump torch from 1.12.0 to 1.13.1

## [v0.6.0] -2022-12-23

### Added

- Enable to save each visualize figure.
  - Interactive operations on the figure are kept because it is saved in html format, not as an image.
- Real-time display of trial number, best value, and Hypervolume in the UI.
- Created UI to set where to save optimization result files.
- SQLite handling in C#
  - Optimize result handling more smooth some case.
  - Previously, it used to read and handle python, which was sometimes slow, but now it calls SQLite directly from CS.
- Only non-dominated trial plot in pareto front.
- Boolean to skip the behavior of checking if the python library is installed in settings.
  - For some reason, the installer may be launched every time even if it is installed, so it can be forcibly skipped in the settings.
- FishEgg component
  - It is now possible to specify an individual of any variable and have it evaluated in the optimization first and foremost, rather than just setting up a variable completely by the sampler when conducting the optimization.
- Warn start CMA-ES support
  - The warm start CMA-ES setting has been added to the CMA-ES settings so that the information from the previous trials can be used.

### Changed

- When there are more than 10 params, the value of "Omit_values" is used instead of "params" to improve the visibility of the ParetoFront plot.
- Default name of optimization result file changed from "Tunny_Opt_Result.db" to "Fish.db".
- Easy-to-understand UI for creating study.
  - Study Create, Continue, and Copy are now clearly separated in the UI.
- Update visualize & output UI
  - Previously, the target study and objective function, variables could not be specified, but now they can be specified.
- Update optuna-dashboard to 0.8.0

### Deprecated

- Disable unused setting tab ui items

### Fixed

- Even if there was an error in the input to the Tunny component, a window could be launched and the button to perform optimization could be pressed, so we made sure that this would not happen.
  - Subject to the following.
    - No input for variable and objective.
    - The name of the objective is not unique.
    - Multiple items entered in Attr.

### Security

- Bump joblib from 1.1.0 to 1.2.0 in /Tunny/Lib
- Bump mako from 1.2.0 to 1.2.2 in /Tunny/Lib

## [0.5.0] -2022-09-03

### Added

- Hypervolume visualization
  - It is useful for determining convergence in multi-objective optimization.
- Clustering visualization
  - Clustering of results during multi-objective optimization makes it easier to evaluate solutions.
- BoTorch Sampler
  - This sampler use Gaussian Process and support multi-objective optimization.
- Quasi-MonteCarlo Sampler
  - [Detail](https://optuna.readthedocs.io/en/latest/reference/samplers/generated/optuna.samplers.QMCSampler.html) about this sampler.
- Support Constraint.
  - Only TPE, GP, NSGAII can use constraint.
- Sampler detail settings UI
  - Previously it was necessary to change the JSON file of the settings, but now it can be changed in the UI
- Enable Text Bake in the FishMarket component.
- Allows selection of NSGA-II crossover methods.
  - `Uniform`, `BLXAlpha`, `SPX`, `SBX`, `VSBX`, `UNDX`
- Ability to set Popsize on CMA-ES restart
- Run GC after trial when has geometry attribute or setting always run.
  - **This change probably make optimize slower before**
  - If you want to cut this setting, set the value of "GcAfterTrial" to 2 in Settings.json.
- Show LICENSE button in Tunny UI.

### Changed

- When genepool is an input, it now creates variable names using nicknames.
- The output of the Pareto solution was made to consider the constraints.
- Multivariate in TPE sampler default option is false to true
- Updated Optuna used to v3.0.0
  - Use `suggest_int` and `suggest_float` instead of `suggest_uniform` for more accurate variable generation in optimization
  - Random and Grid samplers now support multi-objective optimization
  - The format of the db file in which the results are saved has changed. Please note that it is not compatible with the previous one.

### Fixed

- The PythonInstaller window now has no text on the progress bar.
- When more than one Study exists, another Study Name is set and RunOpt no longer causes a Solver Error.
- The error does not occur when the Brep of Geometry of Attribute is null.

## [0.4.0] -2022-07-09

### Added

- Component CI build to get easily development build.
  - If you want to dogfooding, go right ahead!
- Param_FishAttribute component
- Construct Fish Attribute component
  - Component that creates attribute information to be attached to each trial of optimization
- Deconstruct Fish component
  - Component that separates optimization results into Variables, Objectives, and Attributes
- Deconstruct Fish Attribute component
  - Components that output each attribute
- Python package licenses to clearly state the license of each package.
- requirements.txt file to avoid conflict python packages versions.
- Implemented Timeout to stop optimization over time.
- Input components are now highlighted in color, as are other optimization components.

### Changed

- Component location on the Tunny tab.
- The output of the Tunny component is made into Fish, a type that summarizes the results.
- The Geometry input of the Tunny component has been changed to Attribute to allow more attribute information to be handled.
- When restoring the results from Tunny component as Fishes, those that the trial did not work and Objective could not get are not output.
- Update UI
  - The UI of the Restore tab was confusing, so the UI was modified to make it easier to understand which button to press and how the results are output.
- The progress bar on the Output tab has been made to show progress in a more understandable way.
- Error messages are displayed when the input to Tunny component is not appropriate, and inappropriate input wires are automatically removed.
- Error massage in python runtime is more clear.
  - Added error message when the number of objective does not match the existing Study.
  - Added error message when a Study with the same name exists but is used without Loading.

### Fixed

- Stopped sampling when there was no geometry input
- Once optimize output error, the component won't run again
  - I've tried to do a proper Dispose to fix this problem, but it still doesn't work sometimes.
- Optuna-DashBoard and storage relate functions do not work properly when a different Storage path is specified in Settings than the default.
- Pressing the stop button in output and the stop button in Optimize does not stop the operation.
- Components were in their normal color instead of blue.
- Once an error occurs in PythonRuntime, the optimization will not work after that.

## [0.3.0] -2022-05-03

### Added

- Use json to set sampler detail settings
  - Since the settings are saved in Json, the previous values set in the UI remain saved when the window is closed and reopened.
- UI for above detail settings
- If input "-10" in restore model number, Tunny output all result
- Fish component
  - Special GH_Param component for Tunny result to handle its result easier.
  - This is the result of your catching Tunny in a fishery called Optimization.
- FishMarket component
  - Feature to display results side by side in a viewport like a fish market.
- Version info to result rdb file
  - To avoid problems with different versions in the future
- Support Optuna-Dashboard
  - Real-time Web Dashboard for Optuna.
  - https://github.com/optuna/optuna-dashboard#features

### Change

- Combine Tunny component output into one called Fish.
- Component subcategory changed from Util to Tunny
- Model shape data recorded during optimization from a single mesh to multiple GeometryBases.
- Reduced size of distribution package
  - It used to include the dependent PythonPackage from the beginning, but it is now downloaded when it is started on Grasshopper.

## [0.2.0] -2022-05-02

### Added

- Restore progressbar
- Support Galapagos genepool input
- Result reflect button
  - This feature reflecting multi objective optimization result to slider & genepool to use input model number.
  - if input multi model number, the first one is reflect and popup notification about this.
- Support grid sampler

### Changed

- Restore feature was made asynchronous.
- Visualize graph axis name now use input objective's nickname.
- Update supported Rhino version to 7.13.
- Disable optimize window resize.

### Fixed

- Optimization does not stop when the value of Objective is null.
  - When the objective values is null, optimizer try to get another variable and resolve solution.
  - If it is 10 trial to get objectives in 1 optimize loop, optimizer throw error.
- Enable visualize param importances function
  - this function need sklearn, but tunny's python package doesn't include it.
- Optimize window UI is broken when using Hi-DPI environment.
  - Support multi DPI

## [0.1.1] -2022-04-17

### Fixed

- If there is no modelmesh input and use restore function, gh is crashed bug.

## [0.1.0] -2022-04-14

### Added

- Base Tunny component files.
- This CHANGELOG file to hopefully serve as an evolving example of a standardized open source project CHANGELOG.

---

## Template

## [X.Y.Z] -20xx-xx-xx

### Added

for new features.

### Changed

for changes in existing functionality.

### Deprecated

for soon-to-be removed features.

### Removed

for now removed features.

### Fixed

for any bug fixes.

### Security

in case of vulnerabilities.
