drop schema if exists kahoot cascade;
create schema if not exists kahoot;

-- A game represents a quiz/competition
create table kahoot.game (
                             id text primary key,
                             name text not null,
                             current_question_index integer default 0
);

-- Questions belonging to a game
create table kahoot.question (
                                 id text primary key,
                                 game_id text references kahoot.game(id),
                                 question_text text not null,
                                 question_index integer not null,
                                 constraint unique_question_order unique(game_id, question_index)
);

-- Options for each question
create table kahoot.question_option (
                                        id text primary key,
                                        question_id text references kahoot.question(id),
                                        option_text text not null,
                                        is_correct boolean not null
);

-- Players in a game
create table kahoot.player (
                               id text primary key,
                               game_id text references kahoot.game(id),
                               nickname text not null,
                               constraint unique_nickname_per_game unique(game_id, nickname)
);

-- Player answers
create table kahoot.player_answer (
                                      player_id text references kahoot.player(id),
                                      question_id text references kahoot.question(id),
                                      selected_option_id text references kahoot.question_option(id),
                                      answer_timestamp timestamp default current_timestamp,
                                      primary key (player_id, question_id)
);