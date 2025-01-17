namespace MassTransit.Tests.Initializers
{
    using System.Threading.Tasks;
    using Events;
    using MassTransit.Initializers;
    using Metadata;
    using NUnit.Framework;
    using TestFramework;


    [TestFixture]
    public class Initializing_a_regular_class
    {
        [Test]
        public async Task Should_initialize_a_fault()
        {
            InitializeContext<Top> topContext = await MessageInitializerCache<Top>.Initialize(new {Text = "Hello"});

            InitializeContext<Report> context = await MessageInitializerCache<Report>.Initialize(new
            {
                Fault = new FaultEvent<Top>(topContext.Message, NewId.NextGuid(), HostMetadataCache.Host, new IntentionalTestException(),
                    TypeMetadataCache<Top>.MessageTypeNames)
            });

            Assert.That(context.Message, Is.Not.Null);

            Assert.That(context.Message.Fault, Is.Not.Null);
            Assert.That(context.Message.Fault.Host, Is.Not.Null);
            Assert.That(context.Message.Fault.Host.MachineName, Is.EqualTo(HostMetadataCache.Host.MachineName));
            Assert.That(context.Message.Fault.Message, Is.Not.Null);
            Assert.That(context.Message.Fault.Message.Text, Is.EqualTo("Hello"));
        }

        [Test]
        public async Task Should_initialize_all_the_properties()
        {
            InitializeContext<Member> context = await MessageInitializerCache<Member>.Initialize(new
            {
                Name = "Frank",
                Address = new
                {
                    Street = "123 American Way",
                    City = "Dallas"
                }
            });

            Assert.That(context.Message, Is.Not.Null);
            Assert.That(context.Message.Name, Is.EqualTo("Frank"));

            Assert.That(context.Message.Address, Is.Not.Null);
            Assert.That(context.Message.Address.Street, Is.EqualTo("123 American Way"));
            Assert.That(context.Message.Address.City, Is.EqualTo("Dallas"));
        }

        [Test]
        public async Task Should_initialize_with_readonly_properties()
        {
            var model1 = new {ReadWrite = "Some Property Value"};

            var model2 = new ReadWriteReadOnly {ReadWrite = "Some Property Value"};

            InitializeContext<IReadWriteReadOnly> model1_working1 = await MessageInitializerCache<IReadWriteReadOnly>.Initialize(model1);

            InitializeContext<ReadWriteReadOnly> model1_working2 = await MessageInitializerCache<ReadWriteReadOnly>.Initialize(model1);

            InitializeContext<IReadWriteReadOnly> model2_working3 = await MessageInitializerCache<IReadWriteReadOnly>.Initialize(model2);

            InitializeContext<ReadWriteReadOnly> model2_broken1 = await MessageInitializerCache<ReadWriteReadOnly>.Initialize(model2);
        }

        public class ReadWriteReadOnly : IReadWriteReadOnly
        {
            public string ReadWrite { get; set; }
            public string ReadOnly => ReadWrite;
        }


        public interface IReadWriteReadOnly
        {
            string ReadWrite { get; set; }
            string ReadOnly => ReadWrite;
        }


        public interface Report
        {
            Fault<Bottom> Fault { get; }
        }


        public interface Top :
            Bottom
        {
        }


        public interface Bottom
        {
            string Text { get; }
        }


        class Member
        {
            public string Name { get; set; }
            public Address Address { get; private set; }
        }


        class Address
        {
            public string Street { get; private set; }
            public string City { get; set; }
        }
    }
}
