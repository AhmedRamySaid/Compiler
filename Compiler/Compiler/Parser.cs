using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public  Node root;
        
        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0; //make sure its initialized right
            this.TokenStream = TokenStream;
            root = Program();
            return root;
        }
        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(Pre_Program_statements());
            program.Children.Add(Main_Function());
            MessageBox.Show("Success");
            return program;
        }
        Node Pre_Program_statements() {
            //i should first check whether this is a main function or just a normal function
            if (InputPointer + 1 < TokenStream.Count) {
                if (TokenStream[InputPointer + 1].token_type == Token_Class.IDENTIFIER_T)
                {
                    Node Pre_Program_statements_N = new Node("Pre-Program-statements");
                    //if its juts an identifier then its just another function not the main
                    Pre_Program_statements_N.Children.Add((Function_Statement()));
                    Node nextPreProgram = Pre_Program_statements();
                    if (nextPreProgram != null) {
                        Pre_Program_statements_N.Children.Add((nextPreProgram));
                    }
                    return Pre_Program_statements_N;
                }
            }
            return null; //epsilon path
        }

        Node Main_Function() {
            Node Main_Function_N = new Node("Main-Function");

            Main_Function_N.Children.Add(Datatype());
            Main_Function_N.Children.Add(match(Token_Class.MAIN_T));
            Main_Function_N.Children.Add(match(Token_Class.L_PAREN_BRACKET_T));
            Main_Function_N.Children.Add(match(Token_Class.R_PAREN_BRACKET_T));
            Main_Function_N.Children.Add(Function_Body());

            return Main_Function_N;
        }
        Node Datatype() {
            Node Datatype_N = new Node("Datatype");

            if (InputPointer < this.TokenStream.Count) { //check if iam not going out of bound
                Token_Class token = TokenStream[InputPointer].token_type;
                switch (token) {
                    case Token_Class.INT_T:
                        Datatype_N.Children.Add(match(Token_Class.INT_T));
                        break;
                    case Token_Class.FLOAT_T:
                        Datatype_N.Children.Add(match(Token_Class.FLOAT_T));
                        break;
                    case Token_Class.STRING_T:
                        Datatype_N.Children.Add(match(Token_Class.STRING_T));
                        break;
                    default:
                        Errors.Error_List.Add($"Parsing Error: Expected Datatype but found {token} at index {InputPointer}\r\n");
                        InputPointer++; //consume the token
                        break;
                }
            }
            else {
                Errors.Error_List.Add("Parsing Error: Expected Datatype but reached end of file.\r\n"); //no token available
            }
                return Datatype_N;    
        }
        Node Function_Body() {
            Node Function_Body_N = new Node("Function-body");
            Function_Body_N.Children.Add(match(Token_Class.L_CURLY_BRACKET_T));
            Function_Body_N.Children.Add(Statements()); //implement it 
            Function_Body_N.Children.Add(Return_Statement()); //implement it 
            Function_Body_N.Children.Add(match(Token_Class.R_CURLY_BRACKET_T));
            return Function_Body_N;
        }
        Node Function_Statement() {
            Node Function_Statement_N = new Node("Function-Statement");
            Function_Statement_N.Children.Add(Function_Declaration());
            Function_Statement_N.Children.Add(Function_Body());
            return Function_Statement_N;
        }
        Node Function_Declaration() {
            Node Function_Declaration_N = new Node("Function-Declaration");

            Function_Declaration_N.Children.Add(Datatype());
            Function_Declaration_N.Children.Add(match(Token_Class.IDENTIFIER_T));
            Function_Declaration_N.Children.Add(match(Token_Class.L_PAREN_BRACKET_T));
            Node params_node = Params();
            if (params_node != null) {
                Function_Declaration_N.Children.Add(params_node); //prevent orphan nodes
            }
            Function_Declaration_N.Children.Add(match(Token_Class.R_PAREN_BRACKET_T));

            return Function_Declaration_N;
        }
        Node Params() {
            Node Params_N = new Node("Params");

            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                //if this is a datatype, then there is another parameter
                if (token == Token_Class.INT_T || token == Token_Class.FLOAT_T || token == Token_Class.STRING_T) {
                    Params_N.Children.Add(Parameter());
                    Params_N.Children.Add(Comma_Param());
                    return Params_N;
                }
            }
            //epsilon path so we dont cluter the tree even if its not gonna be printed later
            return null;
        }
        Node Parameter() {
            Node Parameter_N = new Node("Parameter");
            Parameter_N.Children.Add(Datatype());
            Parameter_N.Children.Add(match(Token_Class.IDENTIFIER_T));
            return Parameter_N;
        }
        Node Comma_Param() {
            Node Comma_Param_N = new Node("Comma_Param");
            if (InputPointer < this.TokenStream.Count && this.TokenStream[InputPointer].token_type == Token_Class.COMMA_T)
            {
                Comma_Param_N.Children.Add(match(Token_Class.COMMA_T));
                Comma_Param_N.Children.Add(Parameter());

                Node nextCommaParam = Comma_Param();
                if (nextCommaParam != null)
                {
                    Comma_Param_N.Children.Add(nextCommaParam);
                }
                return Comma_Param_N; //only return the node if we actually found a comma
            }

            //epsilon path
            return null;
        }

        Node Statements() {
            Node Statements_N = new Node("Statements");
            //implement it 
            return Statements_N;
        }

        Node Return_Statement() {
            Node Return_Statement_N = new Node("Return_Statement");
            //implement it 
            return Return_Statement_N;
        }


        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString()  + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
