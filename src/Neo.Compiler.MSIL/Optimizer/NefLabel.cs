using System.Diagnostics;

namespace Neo.Compiler.Optimizer
{
    [DebuggerDisplay("Name={Name}, Offset={Offset}")]
    class NefLabel : INefItem
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Offset
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="Offset">Offset</param>
        public NefLabel(string name, int Offset)
        {
            this.Name = name;
            this.Offset = Offset;
        }

        /// <summary>
        /// Set offset
        /// </summary>
        /// <param name="offset">Offset</param>
        public void SetOffset(int offset)
        {
            this.Offset = offset;
        }
    }
}
