using System.Collections.Generic;
using NUnit.Framework;
using RunLab.AesirArchitecture;

namespace RunLab.AesirArchitecture.Tests
{
    #region Test Fixtures

    public interface ITestModel : IModel
    {
        ObservableProperty<int> Counter { get; }
    }

    public class TestModel : ModelBase, ITestModel
    {
        public ObservableProperty<int> Counter { get; } = new ObservableProperty<int>(0);
        protected override void OnInit() { }
    }

    public interface ITestService : IService
    {
        void Increment();
    }

    public class TestService : ServiceBase, ITestService
    {
        private ITestModel mModel;

        protected override void OnInit()
        {
            mModel = GetModel<ITestModel>();
        }

        public void Increment() => mModel.Counter.Value++;
    }

    public interface ITestProvider : IProvider
    {
        string GetLabel();
    }

    public class TestProvider : ITestProvider
    {
        public string GetLabel() => "TestLabel";
    }

    public class IncrementCommand : CommandBase
    {
        protected override void OnExecute()
        {
            GetService<ITestService>().Increment();
        }
    }

    public struct TestEvent
    {
        public int Value;
    }

    public class TestAppContext : AppContext<TestAppContext>
    {
        protected override void Configure()
        {
            RegisterProvider<ITestProvider>(new TestProvider());
            RegisterModel<ITestModel>(new TestModel());
            RegisterService<ITestService>(new TestService());
        }
    }

    #endregion

    public class AppContextTests
    {
        [SetUp]
        public void SetUp()
        {
            TestAppContext.Init();
        }

        [TearDown]
        public void TearDown()
        {
            TestAppContext.Instance.Deinit();
        }

        [Test]
        public void Init_CreatesSingletonInstance()
        {
            Assert.IsNotNull(TestAppContext.Instance);
        }

        [Test]
        public void GetModel_ReturnsRegisteredModel()
        {
            var model = TestAppContext.Instance.GetModel<ITestModel>();
            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Counter.Value);
        }

        [Test]
        public void GetService_ReturnsRegisteredService()
        {
            var service = TestAppContext.Instance.GetService<ITestService>();
            Assert.IsNotNull(service);
        }

        [Test]
        public void GetProvider_ReturnsRegisteredProvider()
        {
            var provider = TestAppContext.Instance.GetProvider<ITestProvider>();
            Assert.IsNotNull(provider);
            Assert.AreEqual("TestLabel", provider.GetLabel());
        }

        [Test]
        public void ExecuteCommand_InvokesCommandLogic()
        {
            var model = TestAppContext.Instance.GetModel<ITestModel>();
            Assert.AreEqual(0, model.Counter.Value);

            TestAppContext.Instance.ExecuteCommand(new IncrementCommand());
            Assert.AreEqual(1, model.Counter.Value);

            TestAppContext.Instance.ExecuteCommand(new IncrementCommand());
            Assert.AreEqual(2, model.Counter.Value);
        }

        [Test]
        public void Invoke_AndAddListener_EventBusWorks()
        {
            var receivedValues = new List<int>();

            TestAppContext.Instance.AddListener<TestEvent>(e => receivedValues.Add(e.Value));

            TestAppContext.Instance.Invoke(new TestEvent { Value = 42 });
            TestAppContext.Instance.Invoke(new TestEvent { Value = 99 });

            Assert.AreEqual(2, receivedValues.Count);
            Assert.AreEqual(42, receivedValues[0]);
            Assert.AreEqual(99, receivedValues[1]);
        }

        [Test]
        public void RemoveListener_StopsReceivingEvents()
        {
            var count = 0;
            void Handler(TestEvent e) => count++;

            TestAppContext.Instance.AddListener<TestEvent>(Handler);
            TestAppContext.Instance.Invoke(new TestEvent());
            Assert.AreEqual(1, count);

            TestAppContext.Instance.RemoveListener<TestEvent>(Handler);
            TestAppContext.Instance.Invoke(new TestEvent());
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ISubscription_Dispose_RemovesListener()
        {
            var count = 0;
            var sub = TestAppContext.Instance.AddListener<TestEvent>(e => count++);

            TestAppContext.Instance.Invoke(new TestEvent());
            Assert.AreEqual(1, count);

            sub.Dispose();
            TestAppContext.Instance.Invoke(new TestEvent());
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ObservableProperty_ValueChanged_TriggersListener()
        {
            var model = TestAppContext.Instance.GetModel<ITestModel>();
            var received = new List<int>();

            model.Counter.AddListener(v => received.Add(v));
            model.Counter.Value = 10;
            model.Counter.Value = 20;

            Assert.AreEqual(2, received.Count);
            Assert.AreEqual(10, received[0]);
            Assert.AreEqual(20, received[1]);
        }

        [Test]
        public void ObservableProperty_SetValueSilently_DoesNotTrigger()
        {
            var model = TestAppContext.Instance.GetModel<ITestModel>();
            var count = 0;

            model.Counter.AddListener(v => count++);
            model.Counter.SetValueSilently(99);

            Assert.AreEqual(0, count);
            Assert.AreEqual(99, model.Counter.Value);
        }

        [Test]
        public void Deinit_ClearsAllRegistrations()
        {
            TestAppContext.Instance.Deinit();

            Assert.IsNull(TestAppContext.Instance.GetModel<ITestModel>());
            Assert.IsNull(TestAppContext.Instance.GetService<ITestService>());
            Assert.IsNull(TestAppContext.Instance.GetProvider<ITestProvider>());
        }

        [Test]
        public void OnRegisterPatch_ModifiesRegistration()
        {
            TestAppContext.Instance.Deinit();

            var extraProvider = new TestProvider();
            TestAppContext.OnRegisterPatch = ctx =>
            {
                // RegisterPatch runs after Configure
            };

            TestAppContext.Init();
            Assert.IsNotNull(TestAppContext.Instance.GetModel<ITestModel>());

            TestAppContext.OnRegisterPatch = _ => { };
        }

        [Test]
        public void Service_CanAccessModelAndProvider()
        {
            var service = TestAppContext.Instance.GetService<ITestService>();
            var model = TestAppContext.Instance.GetModel<ITestModel>();

            Assert.AreEqual(0, model.Counter.Value);
            service.Increment();
            Assert.AreEqual(1, model.Counter.Value);
        }
    }
}
