using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cool_Compiler.Visitor.Semantic.Errors
{
    public interface IErrorLoger
    {
        void LogError(int line, int column, string error);
    }
    public class BasicError
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; }
        public BasicError(int line, int column, string message)
        {
            this.Line = line;
            this.Message = message;
            this.Column = column;
        }
        public override string ToString()
        {
            return "Line: " + this.Line.ToString() + " Column: " + this.Column.ToString() + " " + this.Message;
        }
    }
    public class ErrorLoger : IErrorLoger
    {
        public List<BasicError> Log;

        public ErrorLoger()
        {
            Log = new List<BasicError>();
        }
        public void LogError(int line, int column, string error)
        {
            Log.Add(new BasicError(line, column, error));
        }
    }
}
