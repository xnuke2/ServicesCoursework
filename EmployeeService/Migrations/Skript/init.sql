CREATE TABLE AuthData (
                          Employeeid int4 NOT NULL,
                          Login      varchar(20) NOT NULL UNIQUE,
                          Password   varchar(50) NOT NULL);
CREATE TABLE Benefits (
                          id      SERIAL NOT NULL,
                          Name    varchar(50) NOT NULL UNIQUE,
                          Add_day int4 NOT NULL,
                          PRIMARY KEY (id));
CREATE TABLE Critical_employees (
                                    ID                 SERIAL NOT NULL,
                                    Employee_id_first  int4 NOT NULL,
                                    Employee_id_second int4 NOT NULL,
                                    PRIMARY KEY (ID));
CREATE TABLE Divison (
                         id        SERIAL NOT NULL,
                         Name      varchar(100) NOT NULL,
                         Parent_id int4,
                         PRIMARY KEY (id));
CREATE TABLE Divison_status (
                                ID          SERIAL NOT NULL,
                                Division_id int4 NOT NULL,
                                Year        int4 NOT NULL,
                                Status_id   int4 NOT NULL,
                                End_date    date NOT NULL,
                                PRIMARY KEY (ID));
CREATE TABLE Employee (
                          id         SERIAL NOT NULL,
                          Surname    varchar(20) NOT NULL,
                          Name       varchar(20) NOT NULL,
                          Patronymic varchar(20),
                          Birth_date date NOT NULL,
                          Role_id    int4 NOT NULL,
                          Divison_id int4 NOT NULL,
                          PRIMARY KEY (id));
CREATE TABLE Employee_benefits (
                                   ID          SERIAL NOT NULL,
                                   Start_date  date NOT NULL,
                                   End_date    date NOT NULL,
                                   Employee_id int4 NOT NULL,
                                   Benefits_id int4 NOT NULL,
                                   PRIMARY KEY (ID));
CREATE TABLE Intersection (
                              ID                 SERIAL NOT NULL,
                              Vacation_id_first  int4 NOT NULL,
                              Vacation_id_second int4 NOT NULL,
                              PRIMARY KEY (ID));
CREATE TABLE Role (
                      id   SERIAL NOT NULL,
                      Name varchar(50) NOT NULL UNIQUE,
                      PRIMARY KEY (id));
CREATE TABLE Status (
                        id   SERIAL NOT NULL,
                        Name varchar(20) NOT NULL UNIQUE,
                        PRIMARY KEY (id));
CREATE TABLE Vacation (
                          id                 SERIAL NOT NULL,
                          Start_date         date NOT NULL,
                          End_date           date NOT NULL,
                          Employee_id        int4 NOT NULL,
                          Division_status_ID int4 NOT NULL,
                          PRIMARY KEY (id));
ALTER TABLE Vacation ADD CONSTRAINT FKVacation89610 FOREIGN KEY (Employee_id) REFERENCES Employee (id);
ALTER TABLE Critical_employees ADD CONSTRAINT FKCritical_e424114 FOREIGN KEY (Employee_id_first) REFERENCES Employee (id);
ALTER TABLE Employee ADD CONSTRAINT FKEmployee993156 FOREIGN KEY (Divison_id) REFERENCES Divison (id);
ALTER TABLE Employee_benefits ADD CONSTRAINT FKEmployee_b21845 FOREIGN KEY (Employee_id) REFERENCES Employee (id);
ALTER TABLE Employee_benefits ADD CONSTRAINT FKEmployee_b783711 FOREIGN KEY (Benefits_id) REFERENCES Benefits (id);
ALTER TABLE Critical_employees ADD CONSTRAINT FKCritical_e334488 FOREIGN KEY (Employee_id_second) REFERENCES Employee (id);
ALTER TABLE Intersection ADD CONSTRAINT FKIntersecti675884 FOREIGN KEY (Vacation_id_first) REFERENCES Vacation (id);
ALTER TABLE Employee ADD CONSTRAINT FKEmployee240170 FOREIGN KEY (Role_id) REFERENCES Role (id);
ALTER TABLE Divison ADD CONSTRAINT FKDivison631957 FOREIGN KEY (Parent_id) REFERENCES Divison (id);
ALTER TABLE Intersection ADD CONSTRAINT FKIntersecti767567 FOREIGN KEY (Vacation_id_second) REFERENCES Vacation (id);
ALTER TABLE Divison_status ADD CONSTRAINT FKDivison_st349716 FOREIGN KEY (Division_id) REFERENCES Divison (id);
ALTER TABLE Divison_status ADD CONSTRAINT FKDivison_st291617 FOREIGN KEY (Status_id) REFERENCES Status (id);
ALTER TABLE Vacation ADD CONSTRAINT FKVacation522071 FOREIGN KEY (Division_status_ID) REFERENCES Divison_status (ID);
ALTER TABLE AuthData ADD CONSTRAINT FKAuthData70417 FOREIGN KEY (Employeeid) REFERENCES Employee (id);
