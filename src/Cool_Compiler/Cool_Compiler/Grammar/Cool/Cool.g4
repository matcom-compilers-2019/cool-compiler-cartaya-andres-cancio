grammar Cool;


/* 
* Parser Rules 
*/

program : stamentlist                                                                      # prog
		| (classdefine ';')+                                                               # classlist
		;

classdefine     :       CLASS TYPE (INHERITS TYPE)? '{' (feature ';')* '}'                
                ;

feature         :       method                                                             # featuremet
                |       property                                                           # featureprop
                ;

method			:		ID '(' (formal (',' formal)*)* ')' ':' TYPE '{' expr '}'    
				;

property		:		formal ('<-' expr)? ;



formal : ID ':' TYPE ;
          
expr : expr ('@' TYPE)? '.' ID argexpr              # explicitdispatch
	 | ID argexpr                                   # dispatch
	 | IF expr THEN expr FI                         # if
	 | IF expr THEN expr ELSE expr FI               # ifelse
	 | WHILE expr LOOP expr POOL                    # while
	 | '{' stamentlist '}'                          # corpus
	 | CASE expr OF (branch ';')+ ESAC              # caseof 
	 | NEW TYPE                                     # newtype
	 | op = '~' expr                                # log_arit
	 | ISVOID expr                                  # isvoid
     | expr op = ('*' | '/' ) expr                  # multdiv
     | expr op = ('+' | '-' ) expr                  # sumsub
	 | expr op = ('=' | '<' | '<=') expr            # compar
	 | NOT expr                                     # log_neg
	 | '(' expr ')'                                 # bracket
	 | ID                                           # getid
	 | INT                                          # int
	 | cte = (TRUE | FALSE)                         # bool		 
	 | STRCTE                                       # string
	 | ID '<-' expr                                 # asignacion
	 | LET property (',' property)* IN expr         # let                
	 ;

argexpr : '(' (expr (',' expr)* )* ')';
stamentlist : stament* ;
stament : expr ';' ;
branch : formal IMPLY expr ;
/* 
* Lexer Rules 
*/

/* ID */



/* cte */

STRCTE : '"' (ESC | ~ ["\\])* '"';
INT    : [0-9]+ ;
IMPLY               :           '=>';

/* operator */


EQUAL_AS : '<-';
ADD : '+'    ;
SUB : '-'    ;
POW : '**'   ;
MUL : '*'    ;
DIV : '/'    ;
MOD : '%'    ;
AND : '&'    ;
OR : '|'     ;
XOR : '^'    ;
SLEFT : '<<' ;
SRIGHT : '>>';
LESS  : '<' ;
GREAT : '>' ;
EQUAL_CM : '=' ;
LESSEQ : '<=' ;
GREATEQ : '>=' ;
FIN      : ';' ;


/* keywords */

// key words
CLASS: C L A S S;
ELSE: E L S E ;
FALSE: 'f' A L S E ;
FI: F I ;
IF: I F;
IN: I N;
INHERITS: I N H E R I T S;
ISVOID: I S V O I D;
LET: L E T;
LOOP: L O O P;
POOL: P O O L ;
THEN: T H E N;
WHILE: W H I L E ;
CASE: C A S E ;
ESAC: E S A C;
NEW: N E W;
OF: O F;
NOT: N O T;
TRUE: 't' R U E ;

TYPE                :           [A-Z][_0-9A-Za-z]*;
ID                  :           [a-z][_0-9A-Za-z]*;

fragment A: [aA];
fragment C: [cC];
fragment D: [dD];
fragment E: [eE];
fragment F: [fF];
fragment H: [hH];
fragment I: [iI];
fragment L: [lL];
fragment N: [nN];
fragment O: [oO];
fragment P: [pP];
fragment R: [rR];
fragment S: [sS];
fragment T: [tT];
fragment U: [uU];
fragment V: [vV];
fragment W: [wW];


// skip spaces, tabs, newlines.
WHITESPACE      :   [ \t\r\n\f]+ -> skip; 

// comments
BLOCK_COMMENT   :   '(*' (BLOCK_COMMENT|.)*? '*)'   -> channel(HIDDEN);
LINE_COMMENT    :   '--' .*? ('\n')          -> channel(HIDDEN);

fragment ESC     : '\\' (["\\/bfnrt] | UNICODE);
fragment UNICODE : 'u' HEX HEX HEX HEX;
fragment HEX     : [0-9a-fA-F];