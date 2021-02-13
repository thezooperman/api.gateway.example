-- userschema.usercred definition

-- Drop table

-- DROP TABLE userschema.usercred;

CREATE TABLE userschema.usercred (
	username varchar NOT null unique,
	salt varchar NOT NULL,
	"password" varchar NOT NULL,
	timeidentifier timestamptz(0) NOT NULL DEFAULT CURRENT_TIMESTAMP,
	CONSTRAINT usercred_pk PRIMARY KEY (username)
);