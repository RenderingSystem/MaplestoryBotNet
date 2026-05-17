using ArrayFireNCCTests;
using MaplestoryBotNetTests.Systems.Configuration.Tests;
using MaplestoryBotNetTests.Systems.GPUSelector.Tests;
using MaplestoryBotNetTests.Systems.Keyboard.Tests;
using MaplestoryBotNetTests.Systems.Macro.Tests;
using MaplestoryBotNetTests.Systems.ProcessWatchdog.Tests;
using MaplestoryBotNetTests.Systems.ScreenAilments.Tests;
using MaplestoryBotNetTests.Systems.ScreenCapture.Tests;
using MaplestoryBotNetTests.Systems.ScreenProcessing.Tests;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.Tests;
using MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests;
using MaplestoryBotNetTests.UserInterface.Tests;


void UnitTestSuite()
{
    // Test the Arrayfire NCC library.
    new TestTemplateMatcher().run();
    new GPUSelectorSystemTestSuite().Run();
    // Test the configuration system
    new ConfigurationTestSuite().Run();
    new ConfigurationKeyboardTestSuite().Run();
    new ConfigurationImagesTestSuite().Run();
    new ConfigurationSystemTestSuite().Run();
    new ConfigurationMapModelTestSuite().Run();
    // Test the screen capture system
    new CaptureModuleTestSuite().Run();
    new ScreenCaptureSystemTestSuite().Run();
    new ScreenProcesstingTestSuite().Run();
    new ScreenProcessingSystemTestSuite().Run();
    new ScreenAilmentsSystemTestSuite().Run();
    // Test the keyboard system
    new KeyboardDeviceDetectorTestSuite().Run();
    new KeystrokeTransmitterCommandsTestSuite().Run();
    new KeyboardSystemTestSuite().Run();
    // Test the process watchdog system
    new ProcessWatchdogSystemTestSuite().Run();
    // Test the macro system
    new MacroDataTestSuite().Run();
    new BottingTransmitterTestSuite().Run();
    new RuneingTransmitterTestSuite().Run();
    new SolvingTransmitterTestSuite().Run();
    new CashShopTransmitterTestSuite().Run();
    new AilmentTransmitterTestSuite().Run();
    new MacroSystemTestSuite().Run();
    // Test the main system
    new MainSystemTestSuite().Run();
    // Test the user interface
    new WindowSplashScreenStateHandlersTestSuite().Run();
    new WindowViewStateHandlersTestSuite().Run();
    new WindowMenuItemPopupHandlersTestSuite().Run();
    new UIHandlerSystemTestSuite().Run();
    new WindowSaveLoadMenuHandlersTestSuite().Run();
    new WindowComboBoxScaleHandlersTestSuite().Run();
    new WindowMapEditorHandlersTestSuite().Run();
    new WindowMapEditorRuneHandlersTestSuite().Run();
    new NumericTextBoxHandlersTestSuite().Run();
    new WindowMinimapViewStateHandlerTestSuite().Run();
    new WindowMinimapPositionHandlersTestSuite().Run();
    new WindowRuneingEditorHandlersTestSuite().Run();
    new WindowRuneSolverEditorHandlersTestSuite().Run();
    new WindowAilmentsMenuHandlersTestSuite().Run();
    new WindowPotionsMenuHandlersTests().Run();
}


var thread = new Thread(UnitTestSuite);
thread.SetApartmentState(ApartmentState.STA);
thread.Start();
thread.Join();
