using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pipelines.Test
{
    [UseReporter(typeof(VisualStudioReporter))]
    [TestClass]
    public class PipelineTests
    {
        [TestMethod]
        public void BasicPipelineFunctionalTest()
        {
            Func<string, long> normal = (string age) =>
            {
                // startcode basic_code_line 
                var result = long.Parse(age);
                // endcode 
                return result;
            };
            Func<string, long> piped = age =>
            {
                // startcode basic_pipeline
                var inputPipe = new InputPipe<string>("age");
                var parsePipe = inputPipe.Process(long.Parse);
                var collector = parsePipe.Collect();

                inputPipe.Send("42");
                var result = collector.SingleResult;
                // endcode
                return result;
            };

            Assert.AreEqual(normal("42"), piped("42"));
        }

        [TestMethod]
        public void BasicPipelineTest()
        {
            var input = new InputPipe<string>("age");
            var parse = input.Process(long.Parse);
            var collector = parse.Collect();

            Verify(input);
            input.Send("42");
            Assert.AreEqual(42, collector.SingleResult);
        }

        [TestMethod]
        public void ConnectedPipelinesTest()
        {
            var input = new InputPipe<string>("age");
            var parsePipe = input.Process(long.Parse);
            var collector = parsePipe.Collect();
            parsePipe.Process(LongToString).WithCollector().Process(long.Parse).WithCollector().Process(LongToString)
                .Collect();

            Verify(input);
        }

        [TestMethod]
        public void SplitAndJoin()
        {
            var input = new InputPipe<string>("age");
            var parse = input.Process(long.Parse);
            var longToString = parse.Process(LongToString);
            var incrementLong = parse.Process(IncrementLong);

            var joinedPipes = longToString.JoinTo(incrementLong).Collect();

            Verify(input);
        }

        [TestMethod]
        public void SplitInput()
        {
            var input = new InputPipe<long>("value");
            input.Process(LongToString);
            input.Process(IncrementLong);

            Verify(input);
        }

        [TestMethod]
        public void JoinInputs()
        {
            var input1 = new InputPipe<long>("value1");
            var input2 = new InputPipe<long>("value2");
            var join = input1.JoinTo(input2);
            var collector = join.Collect();

            input1.Send(42);
            Assert.IsTrue(collector.IsEmpty);

            input2.Send(99);
            Assert.AreEqual("(42, 99)", collector.SingleResult.ToString());

            Verify(join);
        }

        private string LongToString(long value)
        {
            return value.ToString();
        }

        private long IncrementLong(long value)
        {
            return value + 1;
        }

        private static void Verify(IGraphNode input)
        {
            Approvals.Verify(WriterFactory.CreateTextWriter(DotGraph.FromPipeline(input), "dot"));
        }
    }

    internal static class _
    {
        public static FunctionPipe<TInput, TOutput> WithCollector<TInput, TOutput>(
            this FunctionPipe<TInput, TOutput> @this)
        {
            @this.Collect();
            return @this;
        }
    }
}