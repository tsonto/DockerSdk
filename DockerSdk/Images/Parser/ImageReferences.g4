grammar ImageReferences;

referenceOnly: reference EOF;

reference: id | name;

id: shortId | mediumId | longId;

shortId: bytex6;

mediumId: bytex32;

longId: 'sha256' COLON mediumId;

name: repository (COLON tag)? (AT digest)?
    ;

digest:
    'sha256' COLON bytex32
    ;

tag:
    alphanum ((alphanum | UNDER | DOT | DASH)* alphanum)?
    ;

repository:
    (firstComponent SLASH)?
    normalComponent
    (SLASH normalComponent)*
    ;
    
firstComponent: normalComponent | hostComponent;    // even if the parser says that it can be considered a normal component, post-parsing might consider it to be a host component

hostComponent:
    hostname (COLON port)?
    ;

hostname:
    label (DOT label)*
    ;

label:
    alphanum (alphanum | DASH)*
    ;

port: DIGIT+;

normalComponent:
    lowAlphanum+ (normalComponentSep lowAlphanum+)*
    ;

normalComponentSep: DOT | UNDER UNDER | UNDER | DASH+;

bytex6: bytex1 bytex1 bytex1 bytex1 bytex1 bytex1;
bytex8: bytex1 bytex1 bytex1 bytex1 bytex1 bytex1 bytex1 bytex1;
bytex32: bytex8 bytex8 bytex8 bytex8;

bytex1: hexChar hexChar;

hexChar: HEXLOWLETTER | DIGIT;
lowLetter: HEXLOWLETTER | NONHEXLOWLETTER;
letter: lowLetter | UPALPHA;
lowAlphanum: lowLetter | DIGIT;
alphanum: letter | DIGIT;

HEXLOWLETTER: [a-f];
NONHEXLOWLETTER: [g-z];
UPALPHA: [A-Z];
DIGIT: [0-9];
AT: '@';
COLON: ':';
DOT: '.';
SLASH: '/';
UNDER: '_';
DASH: '-';
