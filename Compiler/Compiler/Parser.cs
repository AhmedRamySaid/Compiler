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

        //********************E. Program Structure & Functions**********************\\
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

        //*****************************D. Control Flow*****************************\\
        Node IF_Statement() {
            Node IF_Statement_N = new Node("IF_Statement");
            IF_Statement_N.Children.Add(match(Token_Class.IF_T));
            IF_Statement_N.Children.Add(Condition_Statement());
            IF_Statement_N.Children.Add(match(Token_Class.THEN_T));
            IF_Statement_N.Children.Add(Statements());
            IF_Statement_N.Children.Add(E_Statement());

            return IF_Statement_N;
        }

        Node E_Statement()
        {
            Node E_Statement_N = new Node("E_Statement");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.ELSEIF_T)
                {
                    E_Statement_N.Children.Add(Else_If_Statement());
                }
                else if (token == Token_Class.ELSE_T)
                {
                    E_Statement_N.Children.Add(Else_Statement());
                }
                else
                {
                    E_Statement_N.Children.Add(match(Token_Class.END_T));
                }
            }
            else
            {
                //if end of file reached
                Errors.Error_List.Add("Parsing Error: Expected elseif, else, or end, but reached end of file.\r\n");
            }

            return E_Statement_N;
        }

        Node Else_If_Statement() {
            Token_Class token = TokenStream[InputPointer].token_type;
            if(token == Token_Class.ELSEIF_T) {
                //then its an else if statment
                Node Else_If_Statement_N = new Node("Else_If_Statement");
                Else_If_Statement_N.Children.Add(match(Token_Class.ELSEIF_T));
                Else_If_Statement_N.Children.Add(Condition_Statement());
                Else_If_Statement_N.Children.Add(match(Token_Class.THEN_T));
                Else_If_Statement_N.Children.Add(Statements());
                Else_If_Statement_N.Children.Add(E_Statement());

                return Else_If_Statement_N;
            }
            return null;
        }

        Node Else_Statement() {
            Token_Class token = TokenStream[InputPointer].token_type;
            if (token == Token_Class.ELSE_T) {
                Node Else_Statement_N = new Node("Else_Statement");
                Else_Statement_N.Children.Add(match(Token_Class.ELSE_T));
                Else_Statement_N.Children.Add(Statements());
                Else_Statement_N.Children.Add(match(Token_Class.END_T));

                return Else_Statement_N;
            }
            return null;
        }

        Node Repeat_Statement() {
            Token_Class token = TokenStream[InputPointer].token_type;
            if (token == Token_Class.REPEAT_T) {
                Node Repeat_Statement_N = new Node("Repeat_Statement");
                Repeat_Statement_N.Children.Add(match(Token_Class.REPEAT_T));
                Repeat_Statement_N.Children.Add(Statements());
                Repeat_Statement_N.Children.Add(match(Token_Class.UNTIL_T));
                Repeat_Statement_N.Children.Add(Condition_Statement());

                return Repeat_Statement_N;
            }
            return null;
        }


        //******************************C. Block Logic & Statements**********************\\
        Node Statements() {

            Node firstStatement = Statement();
            if (firstStatement != null) {
                Node Statements_N = new Node("Statements");
                Statements_N.Children.Add(firstStatement);

                Node nextStatements = Statements();
                if (nextStatements != null)
                {
                    Statements_N.Children.Add(nextStatements);
                }

                return Statements_N;
            }
            return null; //epsilon path 
        }

        Node Statement(){
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.INT_T || token == Token_Class.FLOAT_T || token == Token_Class.STRING_T) {
                    return Dec_Statement(); //is it a declaation statement
                }
                else if (token == Token_Class.WRITE_T){ //if its a write statement
                    return Write_Statement();
                }
                else if (token == Token_Class.READ_T){
                    return Read_Statement(); //if its a read statement
                }
                else if (token == Token_Class.RETURN_T){
                    return Return_Statement(); //if its a return statement
                }
                else if (token == Token_Class.IF_T){
                    return IF_Statement(); //if its an if statement
                }
                else if (token == Token_Class.REPEAT_T){
                    return Repeat_Statement(); //if its a repeat statement
                }
                else if (token == Token_Class.IDENTIFIER_T) { //check if its a function statement
                    if (InputPointer + 1 < TokenStream.Count)
                    {
                        Token_Class nextToken = TokenStream[InputPointer + 1].token_type;
                        if (nextToken == Token_Class.ASSIGN_T) //identifier + Assign_T then its an assignement
                        {
                            return Assign_Statement();
                        }
                        else if (nextToken == Token_Class.L_PAREN_BRACKET_T) //if i have an identifier token  then left bracket then its a function call statement
                        { 
                            return Function_Call_Statement();
                        }
                    }
                }
            }
            return null; //epsilon path
        }

        Node Assign_Statement() {
            if (InputPointer < TokenStream.Count && (InputPointer+1) < TokenStream.Count) {
                Token_Class token_2 = TokenStream[InputPointer+1].token_type;
                if (token_2 == Token_Class.ASSIGN_T) { //then its an assign statement
                    Node Assign_Statement_N = new Node("Assign_Statement");
                    Assign_Statement_N.Children.Add(match(Token_Class.IDENTIFIER_T));
                    Assign_Statement_N.Children.Add(match(Token_Class.ASSIGN_T));
                    Assign_Statement_N.Children.Add(Expression());
                    Assign_Statement_N.Children.Add(match(Token_Class.SEMICOLON_T));

                    return Assign_Statement_N;
                }
            }
            return null;
        }

        Node Dec_Statement() {
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                //notice i cant use Datatype in here because it consumes the token and i just want to peak
                if (token == Token_Class.INT_T || token == Token_Class.FLOAT_T || token == Token_Class.STRING_T) {
                    Node Dec_Statement_N = new Node("Dec_Statement");
                    Dec_Statement_N.Children.Add(Datatype()); //consume the datatype
                    Dec_Statement_N.Children.Add(Single_Ident());
                    Dec_Statement_N.Children.Add(match(Token_Class.SEMICOLON_T));

                    return Dec_Statement_N;
                }
            }
            return null; //epsilon path
        }
        Node Single_Ident(){
            Node Single_Ident_N = new Node("Single_Ident");

            Single_Ident_N.Children.Add(Ident_Item());
            Node commaNode = Comma_Ident(); //check if its not an epsilon
            if (commaNode != null) {
                Single_Ident_N.Children.Add(commaNode);
            }
            return Single_Ident_N;
        }
        Node Ident_Item() {
            Node Ident_Item_N = new Node("Ident_Item");

            Ident_Item_N.Children.Add(match(Token_Class.IDENTIFIER_T));
            Node trailNode = Ident_Item_Trail(); //Ident_Item_Trail could be an epsilon
            if (trailNode != null){
                Ident_Item_N.Children.Add(trailNode);
            }
            return Ident_Item_N;
        }
        Node Ident_Item_Trail(){

            if (InputPointer < TokenStream.Count){
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.ASSIGN_T){
                    Node Ident_Item_Trail_N = new Node("Ident_Item_Trail");
                    Ident_Item_Trail_N.Children.Add(match(Token_Class.ASSIGN_T));
                    Ident_Item_Trail_N.Children.Add(Expression()); // Assumes you have Expression() implemented

                    return Ident_Item_Trail_N;
                }
            }
            return null; //epsilon path
        }
        Node Comma_Ident(){
            if (InputPointer < TokenStream.Count){
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.COMMA_T){

                    Node Comma_Ident_N = new Node("Comma_Ident");
                    Comma_Ident_N.Children.Add(match(Token_Class.COMMA_T));
                    Comma_Ident_N.Children.Add(Ident_Item());

                    Node nextCommaNode = Comma_Ident(); //call itself recursevly again
                    if (nextCommaNode != null){
                        Comma_Ident_N.Children.Add(nextCommaNode);
                    }

                    return Comma_Ident_N;
                }
            }
            return null; //epsilon path
        }

        Node Write_Statement() {
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.WRITE_T){
                    Node Write_Statement_N = new Node("Write_Statement");
                    Write_Statement_N.Children.Add(match(Token_Class.WRITE_T));
                    Write_Statement_N.Children.Add(Opt_Exp());
                    Write_Statement_N.Children.Add(match(Token_Class.SEMICOLON_T));

                    return Write_Statement_N;
                }
            }
            return null;
        }
        Node Opt_Exp() {
            Node Opt_Exp_N = new Node("Opt_Exp"); //there should be something to be returned anyways
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;

                if (token == Token_Class.ENDL_T) { //check if its an endl
                    Opt_Exp_N.Children.Add(match(Token_Class.ENDL_T));
                }
                else{ //if not and endl then it must be an expression
                    Opt_Exp_N.Children.Add(Expression());
                }
            }
            return Opt_Exp_N;
        }
        Node Read_Statement() {
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.READ_T) { //its a read statement
                    Node Read_Statement_N = new Node("Read_Statement");
                    Read_Statement_N.Children.Add(match(Token_Class.READ_T));
                    Read_Statement_N.Children.Add(match(Token_Class.IDENTIFIER_T));
                    Read_Statement_N.Children.Add(match(Token_Class.SEMICOLON_T));
                    return Read_Statement_N;
                }
            }
            return null;
        }
        Node Function_Call_Statement() {
        //i should make get a unique thing to check on and be sure its a Function_Call_Statement or i return null
            Node Function_Call_Statement_N = new Node("Function_Call_Statement");
            Function_Call_Statement_N.Children.Add(Function_Call());
            Function_Call_Statement_N.Children.Add(match(Token_Class.SEMICOLON_T));
            return Function_Call_Statement_N;
        }
        Node Return_Statement() {
            if (InputPointer < TokenStream.Count)
            {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.RETURN_T)
                { //its a return statement
                    Node Return_Statement_N = new Node("Return_Statement");
                    Return_Statement_N.Children.Add(match(Token_Class.RETURN_T));
                    Return_Statement_N.Children.Add(Expression());
                    Return_Statement_N.Children.Add(match(Token_Class.SEMICOLON_T));
                    return Return_Statement_N;
                }
            }
            return null;
        }



        //************************B. Boolean Logic & Conditions*********************\\

        Node Condition_Statement() {
            Node Condition_Statement_N = new Node("Condition_Statement");
            //implement it
            return Condition_Statement_N;
        }


        //************************A. Mathematical Expressions & Strings****************\\
        Node Expression() {
            Node Expression_N = new Node("Expression");
            //implement it
            return Expression_N;
        }

        Node Function_Call() {
            Node Function_Call_N = new Node("Function_Call");
            //implement it
            return Function_Call_N;
        }


        //*************shared functions************\\

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
