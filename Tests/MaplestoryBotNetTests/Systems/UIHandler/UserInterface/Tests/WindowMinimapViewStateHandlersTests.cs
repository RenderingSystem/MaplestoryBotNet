using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.Systems.ScreenProcessing.SubSystems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNet.ThreadingUtils;
using MaplestoryBotNetTests.Systems.Tests;
using MaplestoryBotNetTests.ThreadingUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests
{
    public class WindowMinimapPositionActionHandlerTests
    {
        private MockDispatcher _dispatcher = new MockDispatcher();

        private TextBox _textBoxX = new TextBox();

        private TextBox _textBoxY = new TextBox();

        private string _templateKey = "";

        private System.Windows.Controls.Image _mapImage = new System.Windows.Controls.Image();

        private BottingModel _bottingModel = new BottingModel();

        public AbstractWindowActionHandler _fixture()
        {
            _dispatcher = new MockDispatcher();
            _textBoxX = new TextBox();
            _textBoxY = new TextBox();
            _templateKey = "lol";
            _mapImage = new System.Windows.Controls.Image
            {
                Source = new WriteableBitmap(
                    1234, 2345, 96, 96, PixelFormats.Bgra32, null
                ),
                Width = 12345,
                Height = 23456
            };
            _bottingModel = new BottingModel();
            return new WindowMinimapPositionActionHandlerFacade(
                _dispatcher,
                _textBoxX,
                _textBoxY,
                _templateKey,
                _mapImage
            );
        }

        /**
         * @brief Verifies that dispatched position updates correctly scale coordinates
         * and store them in the map model.
         * 
         * When a position (123,234) is detected on the minimap, it must be transformed
         * based on the relationship between the source bitmap dimensions (1234x2345) and
         * the actual rendered image size (12345x23456). This test confirms that after
         * dispatching and executing the update action, the map model stores the scaled
         * coordinates (1231,2341) for the template key "lol".
         */
        private void _testDispatchedUpdateSetsModelPosition()
        {
            var fixture = _fixture();
            fixture.Modifier().Modify(
                new WindowMinimapPositionModifierParameters
                {
                    Position = new Tuple<int, int>(123, 234),
                    Model = _bottingModel.GetMapModel(),
                }
            );
            Debug.Assert(_dispatcher.DispatchCalls == 1);
            _dispatcher.DispatchCallArg_action[0]();
            var templatePosition = _bottingModel.GetMapModel().GetTemplatePosition("lol");
            Debug.Assert(templatePosition.Item1 == 1231);
            Debug.Assert(templatePosition.Item2 == 2341);
        }

        /**
         * @brief Verifies that dispatched position updates update the UI text boxes
         * with scaled coordinates.
         * 
         * Beyond updating the model, the handler must also refresh the UI text boxes
         * so users can see the current detected position. This test confirms that
         * after the dispatched action executes, both X and Y text boxes display the
         * scaled coordinates (1231,2341) rather than the raw detected coordinates.
         */
        private void _testDispatchedUpdateSetsTextBoxes()
        {
            var fixture = _fixture();
            fixture.Modifier().Modify(
                new WindowMinimapPositionModifierParameters
                {
                    Position = new Tuple<int, int>(123, 234),
                    Model = _bottingModel.GetMapModel(),
                }
            );
            Debug.Assert(_dispatcher.DispatchCalls == 1);
            _dispatcher.DispatchCallArg_action[0]();
            var templatePosition = _bottingModel.GetMapModel().GetTemplatePosition("lol");
            Debug.Assert(_textBoxX.Text == "1231");
            Debug.Assert(_textBoxY.Text == "2341");
        }

        public void Run()
        {
            _testDispatchedUpdateSetsModelPosition();
            _testDispatchedUpdateSetsTextBoxes();
        }
    }


    public class GameMinimapProcessingSubscriberTests
    {
        private BottingModel _bottingModel = new BottingModel();

        private MockThread _processorThread = new MockThread(new ThreadRunningState());

        private GameMinimapProcessorThreadState _threadState = new GameMinimapProcessorThreadState();

        private Image<Bgra32> _image = new Image<Bgra32>(10, 10);

        private AbstractScreenCaptureSubscriber _fixture(
            string stateKey, string templateKey
        )
        {
            _bottingModel = new BottingModel();
            _image = new Image<Bgra32>(10, 10);
            _threadState = new GameMinimapProcessorThreadState(stateKey);
            _processorThread = new MockThread(new ThreadRunningState());
            _processorThread.ThreadStateReturn.Add(_threadState);
            var fixture = new GameMinimapProcessingSubscriber(
                new SemaphoreSlim(1), templateKey
            );
            fixture.Inject(SystemInjectType.ThreadDependency, _processorThread);
            fixture.Inject(SystemInjectType.BottingModel, _bottingModel);
            fixture.Notify(_image, true);
            return fixture;
        }

        /**
         * @brief Verifies that the subscriber correctly crops the minimap region from the
         * full screenshot and forwards it to the processor thread.
         * 
         * When a new screenshot arrives, the subscriber must extract only the minimap area
         * defined by the map model (coordinates 3,3 to 8,8) and inject this cropped bitmap
         * into the processor thread.
         * 
         * Proper cropping is essential for efficient processing - sending only the minimap
         * region (5x5 pixels in this case) rather than the full screenshot (10x10) reduces
         * processing overhead and ensures template matching focuses only on relevant areas.
         */
        private void _testProcessingImageInjectsCroppedBitmapIntoProcessorThread()
        {
            var fixture = _fixture("lol", "lol");
            _bottingModel.GetMapModel().SetMapArea(3, 3, 8, 8);
            _image[3, 3] = new Bgra32(12, 23, 34);
            fixture.ProcessImage();
            Debug.Assert(_processorThread.InjectCalls == 1);
            var dataType = (int) _processorThread.InjectCallArg_dataType[0];
            var bitmap = (Bitmap) _processorThread.InjectCallArg_data[0]!;
            var pixel = bitmap.GetPixel(0, 0);
            Debug.Assert(dataType == 0x7FFFFFFF);
            Debug.Assert(bitmap.Width == 5);
            Debug.Assert(bitmap.Height == 5);
            Debug.Assert(pixel.R == 12);
            Debug.Assert(pixel.G == 23);
            Debug.Assert(pixel.B == 34);
        }

        /**
         * @brief Verifies that the subscriber only forwards images to processor threads
         * whose state key matches the subscriber's template key.
         * 
         * The system may have multiple minimap processor threads running simultaneously -
         * one for character detection, one for rune detection, etc. Each thread's state
         * contains a template key that identifies what it's looking for. This test creates
         * a subscriber with template key "lol2" but injects a processor thread whose state
         * has key "lol". This key matching prevents screenshots intended for character
         * detection from being mistakenly sent to the rune detector or vice versa.
         */
        private void _testWrongTemplateKeyPreventsImageInjection()
        {
            var fixture = _fixture("lol", "lol2");
            _bottingModel.GetMapModel().SetMapArea(3, 3, 8, 8);
            _image[3, 3] = new Bgra32(12, 23, 34);
            fixture.ProcessImage();
            Debug.Assert(_processorThread.InjectCalls == 0);
        }

        public void Run()
        {
            _testProcessingImageInjectsCroppedBitmapIntoProcessorThread();
            _testWrongTemplateKeyPreventsImageInjection();
        }
    }


    public class WindowMinimapPositionHandlersTestSuite
    {
        public void Run()
        {
            new WindowMinimapPositionActionHandlerTests().Run();
            new GameMinimapProcessingSubscriberTests().Run();
        }
    }
}
