using MaplestoryBotNetTests.Systems.Configuration.Tests;
using MaplestoryBotNetTests.Systems.Keyboard.Tests;
using MaplestoryBotNetTests.Systems.ScreenCapture.Tests;

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