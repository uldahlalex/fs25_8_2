drop schema if exists kahoot cascade;
create schema if not exists kahoot;

create table kahoot.gametemplate (
  id text primary key,
  name text not null  
);

create table kahoot.game
(
    id text primary key,
    template text references kahoot.gametemplate(id) 
);

create table kahoot.player
(
    nickname text not null,
    id text primary key
);

create table kahoot.playergame (
    playerid text references kahoot.player(id),
    gameid text references kahoot.game(id),
    primary key (playerid, gameid)
);

create table kahoot.question
(
    id text primary key,
    gametemplateid text references kahoot.gametemplate(id),
    questiontext text not null
);

create table kahoot.questionoption(
                                      id text primary key,
                                      questionid text references kahoot.question(id),
                                      optiontext text not null,
                                      iscorrect boolean not null
);

create table kahoot.playeranswer(
                                    playerid text references kahoot.player(id),
                                    questionid text references kahoot.question(id),
                                    gameid text references kahoot.game(id),
                                    optionid text references kahoot.questionoption(id),
                                    answertimestamp timestamp default current_timestamp,
                                    primary key (playerid, questionid)
);

create table kahoot.gameround(
    gameid text references kahoot.game(id),
    roundquestionId text references kahoot.question(id),
    id text primary key 
);

alter table kahoot.player owner to avnadmin;
alter table kahoot.question owner to avnadmin;
alter table kahoot.game owner to avnadmin;