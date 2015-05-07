using Xunit;

namespace Moq.Tests
{
	public class MockSequencesFixture
	{
		[Fact]
		public void RightSequenceSuccess()
		{
			var a = new Mock<IFoo>(MockBehavior.Strict);
			var b = new Mock<IFoo>(MockBehavior.Strict);
			var c = new Mock<IFoo>(MockBehavior.Strict);

			var sequence1 = new MockSequence();
			var sequence2 = new MockSequence();
			var sequence3 = new MockSequence();
			a.InSequences(sequence1, sequence2).Setup(x => x.Do(100)).Returns(101);
			b.InSequences(sequence2, sequence3).Setup(x => x.Do(200)).Returns(201);
			c.InSequences(sequence1, sequence3).Setup(x => x.Do(300)).Returns(301);

			a.Object.Do(100);
			b.Object.Do(200);
			c.Object.Do(300);
		}

		[Fact]
		public void InvalidSequenceFail()
		{
			var a = new Mock<IFoo>(MockBehavior.Strict);
			var b = new Mock<IFoo>(MockBehavior.Strict);
			var c = new Mock<IFoo>(MockBehavior.Strict);

			var sequence1 = new MockSequence();
			var sequence2 = new MockSequence();
			var sequence3 = new MockSequence();
			a.InSequences(sequence1, sequence2).Setup(x => x.Do(100)).Returns(101);
			b.InSequences(sequence2, sequence3).Setup(x => x.Do(200)).Returns(201);
			c.InSequences(sequence1, sequence3).Setup(x => x.Do(300)).Returns(301);

			a.Object.Do(100);
			Assert.Throws<MockException>(() => c.Object.Do(300));
		}

		[Fact]
		public void NoCyclicSequenceFail()
		{
			var a = new Mock<IFoo>(MockBehavior.Strict);
			var b = new Mock<IFoo>(MockBehavior.Strict);

			var sequence = new MockSequence();
			a.InSequences(sequence).Setup(x => x.Do(100)).Returns(101);
			b.InSequences(sequence).Setup(x => x.Do(200)).Returns(201);

			Assert.Equal(101, a.Object.Do(100));
			Assert.Equal(201, b.Object.Do(200));

			Assert.Throws<MockException>(() => a.Object.Do(100));
			Assert.Throws<MockException>(() => b.Object.Do(200));
		}

		[Fact]
		public void CyclicSequenceSuccesss()
		{
			var a = new Mock<IFoo>(MockBehavior.Strict);
			var b = new Mock<IFoo>(MockBehavior.Strict);

			var sequence = new MockSequence { Cyclic = true };
			a.InSequences(sequence).Setup(x => x.Do(100)).Returns(101);
			b.InSequences(sequence).Setup(x => x.Do(200)).Returns(201);

			Assert.Equal(101, a.Object.Do(100));
			Assert.Equal(201, b.Object.Do(200));

			Assert.Equal(101, a.Object.Do(100));
			Assert.Equal(201, b.Object.Do(200));

			Assert.Equal(101, a.Object.Do(100));
			Assert.Equal(201, b.Object.Do(200));
		}

		[Fact]
		public void SameMockRightSequenceConsecutiveInvocationsWithSameArguments()
		{
			var a = new Mock<IFoo>(MockBehavior.Strict);

			var sequence = new MockSequence();
			a.InSequences(sequence).Setup(x => x.Do(100)).Returns(101);
			a.InSequences(sequence).Setup(x => x.Do(100)).Returns(102);
			a.InSequences(sequence).Setup(x => x.Do(200)).Returns(201);
			a.InSequences(sequence).Setup(x => x.Do(100)).Returns(103);

			Assert.Equal(101, a.Object.Do(100));
			Assert.Equal(102, a.Object.Do(100));
			Assert.Equal(201, a.Object.Do(200));
			Assert.Equal(103, a.Object.Do(100));
		}

		[Fact]
		public void SameMockRightSequenceSuccess()
		{
			var a = new Mock<IFoo>(MockBehavior.Strict);

			var sequence = new MockSequence();
			a.InSequences(sequence).Setup(x => x.Do(100)).Returns(101);
			a.InSequences(sequence).Setup(x => x.Do(200)).Returns(201);

			Assert.Equal(101, a.Object.Do(100));
			Assert.Equal(201, a.Object.Do(200));
			Assert.Throws<MockException>(() => a.Object.Do(100));
			Assert.Throws<MockException>(() => a.Object.Do(200));
		}

		[Fact]
		public void SameMockInvalidSequenceFail()
		{
			var a = new Mock<IFoo>(MockBehavior.Strict);

			var sequence = new MockSequence();
			a.InSequences(sequence).Setup(x => x.Do(100)).Returns(101);
			a.InSequences(sequence).Setup(x => x.Do(200)).Returns(201);

			Assert.Throws<MockException>(() => a.Object.Do(200));
		}

		public interface IFoo
		{
			int Do(int arg);
		}
	}
}