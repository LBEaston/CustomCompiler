
using System;
using System.IO;

namespace Custom
{
	class Compiler
	{
	    public static void EmitError(string message, string file, int line, int colm)
	    {
			Console.Error.WriteLine(message + " " + file + ":" + line + ":" + colm);
	    }

	    public static void Main(string[] args)
	    {
	        if(args.Length == 0) {
	            Console.Error.WriteLine("No input file(s)");
	            return;
	        }

	        string fileName = args[0];
	        Console.WriteLine(fileName);

	        TokenStream tokenStream = new TokenStream();
	        tokenStream.TokenizeFile(fileName);

			Parser parser = new Parser (tokenStream);
			AstNode rootNode = parser.ParseStream ();

			StreamWriter fout = new StreamWriter ("out.c", false, new System.Text.UTF8Encoding());
			fout.Write ("#include \"stdio.h\"\nint main(int argc, int **argv)\n");
			rootNode.EmitCode (fout);
			fout.Flush ();
	    }
	}
}