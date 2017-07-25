To use the allure nunit 3 test adaptor:

1. Clone the project and build the project.
2. Copy files from build folder into addin folder of the Nunit 3 console runner.
3. In root of nunit 3 console folder add this line to nunit.engine.addins file

addins/allure-nunit3.dll

4. Run an nunit 3 test, an output folder with allure formatted files will be created in the execution root (default is test-output). The output folder can be configured in the settings file / app.config of the package.
