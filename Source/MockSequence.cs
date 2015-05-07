using System;
using System.Linq;
using Moq.Language;

namespace Moq
{
	/// <summary>
	/// Helper class to setup a full trace between many mocks
	/// </summary>
	public class MockSequence
	{
		int sequenceStep;
		int sequenceLength;

		/// <summary>
		/// Initialize a trace setup
		/// </summary>
		public MockSequence()
		{
			sequenceLength = 0;
			sequenceStep = 0;
		}

		/// <summary>
		/// Allow sequence to be repeated
		/// </summary>
		public bool Cyclic { get; set; }

		private void NextStep()
		{
			sequenceStep++;
			if (Cyclic)
				sequenceStep = sequenceStep % sequenceLength;
		}

		internal ISetupConditionResult<TMock> For<TMock>(Mock<TMock> mock)
			where TMock : class
		{
			var expectationPosition = sequenceLength++;

			return mock.When(new Condition(
			   condition: () => expectationPosition == sequenceStep,
			   success: NextStep));
		}

		internal static ISetupConditionResult<TMock> For<TMock>(MockSequence[] sequences, Mock<TMock> mock)
			where TMock : class
		{
			var parts = new Part[sequences.Length];
			for (int index = 0; index < sequences.Length; index++)
			{
				var sequence = sequences[index];
				parts[index] = new Part(sequence.sequenceLength++, sequence);
			}

			return mock.When(new Condition(
			   condition: () => parts.All(part => part.Condition()),
			   success: () => Array.ForEach(parts, part => part.NextStep())));
		}

		struct Part
		{
			private int expectationPosition;
			private MockSequence sequence;

			public Part(int expectationPosition, MockSequence sequence) : this()
			{
				this.expectationPosition = expectationPosition;
				this.sequence = sequence;
			}

			public bool Condition()
			{
				return expectationPosition == sequence.sequenceStep;
			}

			public void NextStep()
			{
				sequence.NextStep();
			}
		}
	}

	/// <summary>
	/// define nice api
	/// </summary>
	public static class MockSequenceHelper
	{
		/// <summary>
		/// Perform an expectation in the trace.
		/// </summary>
		public static ISetupConditionResult<TMock> InSequence<TMock>(
			this Mock<TMock> mock,
			MockSequence sequence)
			where TMock : class
		{
			Guard.NotNull(() => sequence, sequence);

			return sequence.For(mock);
		}

		/// <summary>
		/// Perform an expectation in the trace.
		/// </summary>
		public static ISetupConditionResult<TMock> InSequences<TMock>(
			this Mock<TMock> mock,
			params MockSequence[] sequences)
			where TMock : class
		{
			Guard.NotNull(() => sequences, sequences);
			for (var index = 0; index < sequences.Length; index++)
			{
				var sequence = sequences[index];
				if (sequence == null)
				{
					throw new ArgumentException("sequences contains null sequence at position " + index);
				}

				//for (var other = 0; other < index; other++)
				//{
				//    if (sequence == sequences[other])
				//    {
				//        throw new ArgumentException("sequences contains repeated entry at position " + index + " and " + other);
				//    }
				//}
			}


			return MockSequence.For(sequences, mock);
		}
	}
}
