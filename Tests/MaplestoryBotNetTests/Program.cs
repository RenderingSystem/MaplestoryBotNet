using MaplestoryBotNetTests.Systems.Configuration.Tests;
using MaplestoryBotNetTests.Systems.Keyboard.Tests;
using MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests;
using MaplestoryBotNetTests.Systems.ScreenCapture.Tests;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.UserInterface.Tests;


void UnitTestSuite()
{
    // Test the configuration system
    new ConfigurationTestSuite().Run();
    new ConfigurationKeyboardTestSuite().Run();
    new ConfigurationImagesTestSuite().Run();
    new ConfigurationSystemTestSuite().Run();
    // Test the screen capture system
    new CaptureModuleTestSuite().Run();
    new ScreenCaptureSystemTestSuite().Run();
    // Test the keyboard system
    new KeyboardDeviceDetectorTestSuite().Run();
    new KeystrokeTransmitterTestSuite().Run();
    new KeyboardSystemTestSuite().Run();
    // Test the macro system
    new MacroTranslatorTestSuite().Run();
    new ScriptedMacroAgentTestSuite().Run();
    new AbstractMacroAgentTestSuite().Run();
    // Test the main system
    new MainSystemTestSuite().Run();
    // Test the user interface
    new WindowSplashScreenStateHandlersTestSuite().Run();
    new WindowViewStateHandlersTestSuite().Run();
    new WIndowMenuItemPopupHandlersTestSuite().Run();
}


var thread = new Thread(UnitTestSuite);
thread.SetApartmentState(ApartmentState.STA);
thread.Start();
thread.Join();
