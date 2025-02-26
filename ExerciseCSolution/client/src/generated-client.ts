import { BaseDto } from 'ws-request-hook';
//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v14.2.0.0 (NJsonSchema v11.1.0.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

/* tslint:disable */
/* eslint-disable */
// ReSharper disable InconsistentNaming




export interface MemberHasLeftDto extends BaseDto {
    memberId?: string;
}

export interface AdminWantsToStartGameDto extends BaseDto {
    password?: string;
}

export interface ServerAddsClientToGameDto extends BaseDto {
    gameId?: string;
}

export interface AdminHasStartedGameDto extends BaseDto {
    gameId?: string;
}

export interface AdminWantsToStartQuestionDto extends BaseDto {
    password?: string;
    gameId?: string;
}

export interface ServerSendsQuestionDto extends BaseDto {
    question?: QuestionDTO;
}

export interface QuestionDTO {
    questionId?: string;
    questionText?: string;
    isAnswered?: boolean;
    options?: QuestionOptionDTO[];
    playerAnswers?: PlayerAnswerDTO[];
}

export interface QuestionOptionDTO {
    optionId?: string;
    optionText?: string;
    isCorrect?: boolean;
}

export interface PlayerAnswerDTO {
    playerId?: string;
    playerNickname?: string;
    selectedOptionId?: string | undefined;
    isCorrect?: boolean;
    answerTimestamp?: Date | undefined;
}

export interface ServerEndsGameRoundDto extends BaseDto {
    gameStateDto?: GameStateDTO;
}

export interface GameStateDTO {
    gameId?: string;
    playerScores?: PlayerScoreDTO[];
}

export interface PlayerScoreDTO {
    playerId?: string;
    nickname?: string;
    correctAnswers?: number;
}

export interface ServerEndsGameDto extends BaseDto {
    gameStateDto?: GameStateDTO;
}

export interface ClientEntersLobbyDto extends BaseDto {
}

export interface ServerHasClientInLobbyDto extends BaseDto {
    allClientIds?: string[];
}

export interface ClientAnswersQuestionDto extends BaseDto {
    optionId?: string;
    questionId?: string;
    gameId?: string;
}

export interface ClientWantsToAuthenticateDto extends BaseDto {
    username?: string;
}

export interface ClientWantsToSubscribeToTopicDto extends BaseDto {
    topicId?: string;
}

export interface ClientWantsToUnsubscribeFromTopicDto extends BaseDto {
    topicId?: string;
}

export interface ServerAuthenticatesClientDto extends BaseDto {
    topics?: string[];
}

export interface ServerConfirmsDto extends BaseDto {
    success?: boolean;
}

export interface ServerSendsErrorMessageDto extends BaseDto {
    error?: string;
}

/** Available eventType constants */
export enum StringConstants {
    MemberHasLeftDto = "MemberHasLeftDto",
    AdminWantsToStartGameDto = "AdminWantsToStartGameDto",
    ServerAddsClientToGameDto = "ServerAddsClientToGameDto",
    AdminHasStartedGameDto = "AdminHasStartedGameDto",
    AdminWantsToStartQuestionDto = "AdminWantsToStartQuestionDto",
    ServerSendsQuestionDto = "ServerSendsQuestionDto",
    ServerEndsGameRoundDto = "ServerEndsGameRoundDto",
    ServerEndsGameDto = "ServerEndsGameDto",
    ClientEntersLobbyDto = "ClientEntersLobbyDto",
    ServerHasClientInLobbyDto = "ServerHasClientInLobbyDto",
    ClientAnswersQuestionDto = "ClientAnswersQuestionDto",
    ClientWantsToAuthenticateDto = "ClientWantsToAuthenticateDto",
    ClientWantsToSubscribeToTopicDto = "ClientWantsToSubscribeToTopicDto",
    ClientWantsToUnsubscribeFromTopicDto = "ClientWantsToUnsubscribeFromTopicDto",
    ServerAuthenticatesClientDto = "ServerAuthenticatesClientDto",
    ServerConfirmsDto = "ServerConfirmsDto",
    ServerSendsErrorMessageDto = "ServerSendsErrorMessageDto",
}

