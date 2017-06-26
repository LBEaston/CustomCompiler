
using System;

class Compiler
{
    public static void EmitError(string message, string file, int line, int colm)
    {

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

        // Ast_Node *root_node = parse_stream(&token_stream);

        // emit_code(stdout, root_node);

        // for(s32 i = 0; i < sb_count(token_stream.tokens); ++i) {
        //     Token *token = &token_stream.tokens[i];
        //     print_token(token);
        // }
    }
}
