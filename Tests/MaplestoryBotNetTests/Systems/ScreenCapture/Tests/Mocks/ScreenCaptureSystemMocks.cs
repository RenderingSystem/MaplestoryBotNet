using MaplestoryBotNet.Systems.ScreenCapture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.ScreenCapture.Tests.Mocks
{
    public class MockScreenCaptureStore : AbstractScreenCaptureStore
    {
        public List<string> CallOrder = [];

        public int GetLatestCalls = 0;
        public int GetLatestIndex = 0;
        public List<Image<Bgra32>?> GetLatestReturn = [];
        public override Image<Bgra32>? GetLatest()
        {
            var callReference = new TestUtilities().Reference(this) + "GetLatest";
            CallOrder.Add(callReference);
            GetLatestCalls++;
            if (GetLatestIndex < GetLatestReturn.Count)
                return GetLatestReturn[GetLatestIndex++];
            throw new IndexOutOfRangeException();
        }

        public int SetLatestCalls = 0;
        public List<Image<Bgra32>> SetLatestCallArg_image = [];
        public override void SetLatest(Image<Bgra32> image)
        {
            var callReference = new TestUtilities().Reference(this) + "SetLatest";
            CallOrder.Add(callReference);
            SetLatestCalls++;
            SetLatestCallArg_image.Add(image);
        }
    }

    public class MockScreenCaptureSubscriber : AbstractScreenCaptureSubscriber
    {
        public List<string> CallOrder = [];

        public MockScreenCaptureSubscriber(SemaphoreSlim semaphore) : base(semaphore)
        {

        }

        public int WaitForNotificationCalls = 0;
        public bool WaitForNotificationSpy = false;
        public override void WaitForNotification()
        {
            var callReference = new TestUtilities().Reference(this) + "WaitForNotification";
            CallOrder.Add(callReference);
            WaitForNotificationCalls++;
            if (WaitForNotificationSpy)
                base.WaitForNotification();

        }

        public int ProcessImageCalls = 0;
        public override void ProcessImage()
        {
            var callReference = new TestUtilities().Reference(this) + "ProcessImage";
            CallOrder.Add(callReference);
            ProcessImageCalls++;
        }

        public int NotifyCalls = 0;
        public bool NotifySpy = false;
        public List<Image<Bgra32>> NotifyCallArg_image = [];
        public List<bool> NotifyCallArg_updated = [];
        public override void Notify(Image<Bgra32> image, bool updated)
        {
            var callReference = new TestUtilities().Reference(this) + "Notify";
            CallOrder.Add(callReference);
            NotifyCalls++;
            NotifyCallArg_image.Add(image);
            NotifyCallArg_updated.Add(updated);
            if (NotifySpy)
                base.Notify(image, updated);
        }
    }


    public class MockScreenCapturePublisherCountDown : AbstractScreenCapturePublisherCountDown
    {
        public List<string> CallOrder = [];

        public int CountDownCalls = 0;
        public override void CountDown()
        {
            var callReference = new TestUtilities().Reference(this) + "CountDown";
            CallOrder.Add(callReference);
            CountDownCalls++;
        }

        public int SetCountDownCalls = 0;
        public List<int> SetCountDownCallArg_countDown = [];
        public override void SetCountDown(int countDown)
        {
            var callReference = new TestUtilities().Reference(this) + "SetCountDown";
            CallOrder.Add(callReference);
            SetCountDownCalls++;
            SetCountDownCallArg_countDown.Add(countDown);
        }

        public int WaitCountDownCalls = 0;
        public override void WaitCountDown()
        {
            var callReference = new TestUtilities().Reference(this) + "WaitCountDown";
            CallOrder.Add(callReference);
            WaitCountDownCalls++;
        }
    }


    public class MockScreenCapturePublisher : AbstractScreenCapturePublisher
    {
        public List<string> CallOrder = [];

        public int NotifyCompleteCalls = 0;
        public override void NotifyComplete()
        {
            var callReference = new TestUtilities().Reference(this) + "NotifyComplete";
            CallOrder.Add(callReference);
            NotifyCompleteCalls++;
        }

        public int PublishCalls = 0;
        public List<Image<Bgra32>> PublishCallArg_image = [];
        public List<bool> PublishCallArg_updated = [];
        public override void Publish(Image<Bgra32> image, bool updated)
        {
            var callReference = new TestUtilities().Reference(this) + "Publish";
            CallOrder.Add(callReference);
            PublishCalls++;
            PublishCallArg_image.Add(image);
            PublishCallArg_updated.Add(updated);
        }
    }
}
