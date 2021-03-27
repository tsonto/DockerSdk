grammar ImageReferences;

referenceonly: reference EOF;

reference: id | name;

id: shortid | mediumid | longid;

shortid: bytex6;

mediumid: bytex32;

longid: HASH mediumid;

name: repository tag? digest?
    ;

digest:
    AT HASH bytex32
    ;

tag:
    TAGCH (TAGCHIN* TAGCH)?
    ;

repository:
    hostcomponent?
    normalcomponent+;
    
hostcomponent:
    hostname (COLON port)?
    ;

hostname:
    label (DOT label)*
    ;

label:
    HOSTCHARF HOSTCHAR*
    ;

port: DIGIT+;

normalcomponent:
    NCH+ (SEP NCH+)*
    ;

bytex6: BYTE BYTE BYTE BYTE BYTE BYTE;
bytex8: BYTE BYTE BYTE BYTE BYTE BYTE BYTE BYTE;
bytex32: bytex8 bytex8 bytex8 bytex8;

BYTE: [0-9a-f][0-9a-f];
TAGCH: [a-zA-Z0-9];
TAGCHIN: [a-zA-Z0-9_.-];
COLON: ':';
AT: '@';
HASH: 'sha265:';
DOT: '.';
HOSTCHARF: [a-zA-Z0-9];
HOSTCHAR: [a-zA-Z0-9-];
DIGIT: [0-9];
NCH: [a-z0-9];
SEP: '.' | '__' | '_' | '-'+;
