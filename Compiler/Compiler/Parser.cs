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
        public int InputPointer = 0;
       public  List<Token> TokenStream;
        public  Node root;
        
        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0; //make sure its initialized right
            this.TokenStream = TokenStream;
            root = Program();
            return root;
        }
        //just for testing
        public void InitializeForTest(List<Token> tokens)
        {
            TokenStream = tokens;
            InputPointer = 0;
            Errors.Error_List.Clear(); // Clear any previous errors
        }

        //********************E. Program Structure & Functions**********************\\
        public Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(Pre_Program_statements());
            program.Children.Add(Main_Function());
            MessageBox.Show("Success");
            return program;
        }
        public Node Pre_Program_statements() {
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

        public Node Main_Function() {
            Node Main_Function_N = new Node("Main-Function");

            Main_Function_N.Children.Add(Datatype());
            Main_Function_N.Children.Add(match(Token_Class.MAIN_T));
            Main_Function_N.Children.Add(match(Token_Class.L_PAREN_BRACKET_T));
            Main_Function_N.Children.Add(match(Token_Class.R_PAREN_BRACKET_T));
            Main_Function_N.Children.Add(Function_Body());

            return Main_Function_N;
        }
        public Node Datatype() {
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
        public Node Function_Body() {
            Node Function_Body_N = new Node("Function-body");
            Function_Body_N.Children.Add(match(Token_Class.L_CURLY_BRACKET_T));
            Function_Body_N.Children.Add(Statements()); //implement it 
            Function_Body_N.Children.Add(Return_Statement()); //implement it 
            Function_Body_N.Children.Add(match(Token_Class.R_CURLY_BRACKET_T));
            return Function_Body_N;
        }
        public Node Function_Statement() {
            Node Function_Statement_N = new Node("Function-Statement");
            Function_Statement_N.Children.Add(Function_Declaration());
            Function_Statement_N.Children.Add(Function_Body());
            return Function_Statement_N;
        }
        public Node Function_Declaration() {
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
        public Node Params() {
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
        public Node Parameter() {
            Node Parameter_N = new Node("Parameter");
            Parameter_N.Children.Add(Datatype());
            Parameter_N.Children.Add(match(Token_Class.IDENTIFIER_T));
            return Parameter_N;
        }
        public Node Comma_Param() {
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
        public Node IF_Statement() {
            Node IF_Statement_N = new Node("IF_Statement");
            IF_Statement_N.Children.Add(match(Token_Class.IF_T));
            IF_Statement_N.Children.Add(Condition_Statement());
            IF_Statement_N.Children.Add(match(Token_Class.THEN_T));
            IF_Statement_N.Children.Add(Statements());
            IF_Statement_N.Children.Add(E_Statement());

            return IF_Statement_N;
        }

        public Node E_Statement() {
            Node E_Statement_N = new Node("E_Statement");
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.ELSEIF_T)  {
                    E_Statement_N.Children.Add(Else_If_Statement());
                }
                else if (token == Token_Class.ELSE_T)  {
                    E_Statement_N.Children.Add(Else_Statement());
                }
                else{
                    E_Statement_N.Children.Add(match(Token_Class.END_T));
                }
            }
            else {
                //if end of file reached
                Errors.Error_List.Add("Parsing Error: Expected elseif, else, or end, but reached end of file.\r\n");
            }

            return E_Statement_N;
        }

        public Node Else_If_Statement() {
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

        public Node Else_Statement() {
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

        public Node Repeat_Statement() {
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
        public Node Statements() {

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

        public Node Statement(){
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

        public Node Assign_Statement() {
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

        public Node Dec_Statement() {
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
        public Node Single_Ident(){
            Node Single_Ident_N = new Node("Single_Ident");

            Single_Ident_N.Children.Add(Ident_Item());
            Node commaNode = Comma_Ident(); //check if its not an epsilon
            if (commaNode != null) {
                Single_Ident_N.Children.Add(commaNode);
            }
            return Single_Ident_N;
        }
        public Node Ident_Item() {
            Node Ident_Item_N = new Node("Ident_Item");

            Ident_Item_N.Children.Add(match(Token_Class.IDENTIFIER_T));
            Node trailNode = Ident_Item_Trail(); //Ident_Item_Trail could be an epsilon
            if (trailNode != null){
                Ident_Item_N.Children.Add(trailNode);
            }
            return Ident_Item_N;
        }
        public Node Ident_Item_Trail(){

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
        public  Node Comma_Ident(){
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

        public Node Write_Statement() {
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
        public Node Opt_Exp() {
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
        public Node Read_Statement() {
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
        public Node Function_Call_Statement() {
        
            Node Function_Call_Statement_N = new Node("Function_Call_Statement");
            Function_Call_Statement_N.Children.Add(match(Token_Class.IDENTIFIER_T));//eat this idiot
            Function_Call_Statement_N.Children.Add(Function_Call_Trail());
            Function_Call_Statement_N.Children.Add(match(Token_Class.SEMICOLON_T));
            return Function_Call_Statement_N;
        }
        public Node Return_Statement() {
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
        public Node Bool_Operator() {
            Node Bool_Operator_N = new Node("Bool_Operator");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.LOGIC_AND_T)
                {
                    Bool_Operator_N.Children.Add(match(Token_Class.LOGIC_AND_T));
                }
                else if (token == Token_Class.LOGIC_OR_T)
                {
                    Bool_Operator_N.Children.Add(match(Token_Class.LOGIC_OR_T));
                }
                else {
                    //catch and consume if its not a real bool operator token
                    Errors.Error_List.Add($"Parsing Error: Expected Boolean Operator (&&, ||) but found {token} \r\n");
                    InputPointer++;
                }
            }
            return Bool_Operator_N;
        }
        public Node Condition_Operator() {
            Node Condition_Operator_N = new Node("Condition_Operator");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class token = TokenStream[InputPointer].token_type;
                switch (token) {
                    case Token_Class.LESS_THAN_T:
                    case Token_Class.GREATER_THAN_T:
                    case Token_Class.EQUAL_T:
                    case Token_Class.NOT_EQUAL_T:
                        //all use the same line
                        Condition_Operator_N.Children.Add(match(token));
                        break;
                    default:
                        //error catch
                        Errors.Error_List.Add($"Parsing Error: Expected Condition Operator (<, >, =, !=) but found {token} \r\n");
                        InputPointer++; //consume the wrong token
                        break;
                }
            }
            return Condition_Operator_N;
        }
        public Node Condition() {
            Node Condition_N = new Node("Condition");
            Condition_N.Children.Add(match(Token_Class.IDENTIFIER_T));
            Condition_N.Children.Add(Condition_Operator());
            Condition_N.Children.Add(Term());
            return Condition_N;
        }
        public Node Condition_Statement() {
            Node Condition_Statement_N = new Node("Condition_Statement");
            Condition_Statement_N.Children.Add(Condition());
            Condition_Statement_N.Children.Add(Bool_stat());
            return Condition_Statement_N;
        }
        public Node Bool_stat() {
            if (InputPointer < TokenStream.Count)
            {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.LOGIC_AND_T || token == Token_Class.LOGIC_OR_T) {
                    Node Bool_stat_N = new Node("Bool_stat");

                    Bool_stat_N.Children.Add(Bool_Operator());
                    Bool_stat_N.Children.Add(Condition());

                    Node next_Bool_stat = Bool_stat(); //recursive call
                    if (next_Bool_stat != null) {
                        Bool_stat_N.Children.Add(next_Bool_stat);
                    }
                    return Bool_stat_N;
                }
            }
            return null;
        }

        //************************A. Mathematical Expressions & Strings****************\\

        public Node Expression() {
            Node Expression_N = new Node("Expression");
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                //check if its a string literal
                if (token == Token_Class.STRING_LITERAL_T) {
                    Expression_N.Children.Add(match(Token_Class.STRING_LITERAL_T));
                    return Expression_N;
                }
                //other options: Equation or Term
                Node parsedCore = Math_Core();
                if (InputPointer < TokenStream.Count) {
                    Token_Class nextToken = TokenStream[InputPointer].token_type;

                    //if its a math operator, this isnt just a Term... its a full Equation!!
                    if (nextToken == Token_Class.PLUS_T || nextToken == Token_Class.MINUS_T ||
                        nextToken == Token_Class.MULTIPLY_T || nextToken == Token_Class.DIVIDE_T) {

                        //pass our parsedCore into Equation so we dont lose it!!
                        Expression_N.Children.Add(Equation(parsedCore));
                        return Expression_N;
                    }
                } 
                // its just a single term or a math core since there is no operators
                Expression_N.Children.Add(parsedCore);
                return Expression_N;
            }
            return null;
        }
        public Node Equation(Node preParsed_Math_Core = null){
            Node Equation_N = new Node("Equation");
            if (preParsed_Math_Core != null){
                //already parsed it in Expression() so we just pass it to here and attach it 
                Equation_N.Children.Add(preParsed_Math_Core);
            }
            else {
                //else its a new bracket
                Equation_N.Children.Add(Math_Core());
            }
            //we must have another term after the arith operator 
            Equation_N.Children.Add(Arith_Operator());
            Equation_N.Children.Add(Math_Core());

            Node trailNode = E_Trail();
            if (trailNode != null)
            {
                Equation_N.Children.Add(trailNode);
            }

            return Equation_N;
        }
        public Node Arith_Operator() {
            Node Arith_Operator_N = new Node("Arith-Operator");
            if (InputPointer < TokenStream.Count)
            {
                Token_Class token = TokenStream[InputPointer].token_type;

                if (token == Token_Class.PLUS_T || token == Token_Class.MINUS_T ||
                    token == Token_Class.MULTIPLY_T || token == Token_Class.DIVIDE_T) {
                    Arith_Operator_N.Children.Add(match(token));
                    return Arith_Operator_N;
                }
            }
            return null;
        }
       public Node Math_Core() {
            Node Math_Core_N = new Node("Math_Core");
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;

                if (token == Token_Class.L_PAREN_BRACKET_T) { //if its a left parantethesis then the other option
                    Math_Core_N.Children.Add(match(Token_Class.L_PAREN_BRACKET_T));
                    Math_Core_N.Children.Add(Equation()); // Safely parses inner equation
                    Math_Core_N.Children.Add(match(Token_Class.R_PAREN_BRACKET_T));
                    return Math_Core_N;
                }
                else { //if its not a left paren then it must be a term
                    Node termNode = Term(); 
                    if (termNode != null) {
                        Math_Core_N.Children.Add(termNode);
                        return Math_Core_N;
                    }
                }
            }
            return null;
        }

       public  Node E_Trail() {
            if (InputPointer < TokenStream.Count){
                Token_Class token = TokenStream[InputPointer].token_type;
                //if its a math operator then we are good...not the epsilon path
                if (token == Token_Class.PLUS_T || token == Token_Class.MINUS_T ||
                    token == Token_Class.MULTIPLY_T || token == Token_Class.DIVIDE_T) {

                    Node E_Trail_N = new Node("E_Trail");
                    E_Trail_N.Children.Add(Arith_Operator());
                    E_Trail_N.Children.Add(Math_Core());

                    Node nextTrail = E_Trail();//recursive call
                    if (nextTrail != null){
                        E_Trail_N.Children.Add(nextTrail);
                    }
                    return E_Trail_N;
                }
            }
            //epsilon path
            return null;
        }
        public Node Function_Call_Trail() {
            Node Function_Call_N = new Node("Function_Call");

            Function_Call_N.Children.Add(match(Token_Class.L_PAREN_BRACKET_T));
            Node argsNode = Args();
            if (argsNode != null) {
                Function_Call_N.Children.Add(argsNode);
            }
            Function_Call_N.Children.Add(match(Token_Class.R_PAREN_BRACKET_T));
            return Function_Call_N;
        }
        public Node Args(){
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.R_PAREN_BRACKET_T) { //if its a right bracket then no another arg
                    return null;
                } //else its an expression
                Node Args_N = new Node("Args");
                Args_N.Children.Add(Expression()); 

                Node commaArgNode = Comma_Arg();//check for more args
                if (commaArgNode != null) {
                    Args_N.Children.Add(commaArgNode);
                }
                return Args_N;
            }
            return null;
        }
        public Node Comma_Arg() {
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.COMMA_T) {
                    Node Comma_Arg_N = new Node("Comma_Arg");

                    Comma_Arg_N.Children.Add(match(Token_Class.COMMA_T));
                    Comma_Arg_N.Children.Add(Expression());
                    Node nextCommaArg = Comma_Arg(); //recursive call
                    if (nextCommaArg != null) {
                        Comma_Arg_N.Children.Add(nextCommaArg);
                    }
                    return Comma_Arg_N;
                }
            }
            return null; //epsilon path
        }

        public Node Term() {
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                //check if its a Number_Literal
                if (token == Token_Class.INT_LITERAL_T || token == Token_Class.FLOAT_LITERAL_T) {
                    Node Term_N = new Node("Term");
                    Term_N.Children.Add(Number_Literal());
                    return Term_N;
                }
                //check if its an identifier then its supposed to be a Term_trail...(either just a variable or a function call)
                else if (token == Token_Class.IDENTIFIER_T) {
                    Node Term_N = new Node("Term");
                    Term_N.Children.Add(match(Token_Class.IDENTIFIER_T));

                    Node trailNode = Term_Trail();
                    if (trailNode != null) {
                        Term_N.Children.Add(trailNode);
                    }
                    return Term_N;
                }
            }
            return null;
        }
        public Node Number_Literal() {
            if (InputPointer < TokenStream.Count)
            {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.INT_LITERAL_T || token == Token_Class.FLOAT_LITERAL_T)
                {
                    Node Number_Literal_N = new Node("Number_Literal");
                    Number_Literal_N.Children.Add(match(token));
                    return Number_Literal_N;
                }
            }
            else { //reached end of tokens 
                Errors.Error_List.Add("Parsing Error: Expected INT_LITERAL_T or FLOAT_LITERAL_T but reached end of file.\r\n");
            }
            return null;
        }
        public Node Term_Trail() {
            if (InputPointer < TokenStream.Count) {
                Token_Class token = TokenStream[InputPointer].token_type;
                if (token == Token_Class.L_PAREN_BRACKET_T) {
                    Node Term_Trail_N = new Node("Term_Trail");
                    Term_Trail_N.Children.Add(Function_Call_Trail());
                    return Term_Trail_N;
                }
            }
            return null; //epsilon path
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
