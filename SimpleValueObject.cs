using System;
using System.Collections.Generic;

namespace EfCoreTest {
    public class SimpleValueObject : ValueObjectBase {
        public string Value { get; private set; } = null!;

        private SimpleValueObject() {
        }

        /// <summary>
        /// Simple value object
        /// </summary>
        public static SimpleValueObject Create() {
            return new SimpleValueObject {
                Value = $"Code {new Random().Next()}",
            };
        }

        protected override IEnumerable<object> GetAtomicValues() {
            yield return Value;
        }

        public override string ToString() {
            return Value;
        }
    }
}
