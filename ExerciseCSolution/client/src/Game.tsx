import {useEffect, useState} from "react";
import {useWsClient} from "ws-request-hook";
import {
    ClientAnswersQuestionDto,
    GameStateDTO,
    QuestionDTO,
    ServerEndsGameDto,
    ServerEndsGameRoundDto,
    ServerSendsQuestionDto,
    StringConstants
} from "./generated-client.ts";
import toast from "react-hot-toast";

export default function Game() {

    const {onMessage, sendRequest, send, readyState} = useWsClient();
    const [gameId, setGameId] = useState<string | undefined>(undefined);
    const [currentQuestion, setCurrentQuestion] = useState<QuestionDTO | undefined>(undefined)
    const [gameState, setGameState] = useState<GameStateDTO | undefined>(undefined)
    
    useEffect(() => {
        if (readyState != 1)
            return;
        onMessage<ServerSendsQuestionDto>(StringConstants.ServerSendsQuestionDto, (dto) => {
            setCurrentQuestion(dto.question)
            toast('NEW QUESTION!!!')
        });
        onMessage<ServerEndsGameDto>(StringConstants.ServerEndsGameDto, (dto) => {
            toast("game has ended")
            setCurrentQuestion(undefined)
            setGameState(dto.gameStateDto)
        })
        onMessage<ServerEndsGameRoundDto>(StringConstants.ServerEndsGameRoundDto, (dto) => {
            toast("game round has ended")
            setCurrentQuestion(undefined)
            setGameState(dto.gameStateDto)
        })
    }, [readyState]);

    return (
        <>
            <div className="w-full h-52 flex items-center justify-center flex-col">

                {
                    currentQuestion ?

                        <>

                            <div>{currentQuestion.questionText}</div>
                            {currentQuestion.options?.map(opt =>
                                <div key={opt.optionId}>
                                    <button className="btn btn-secondary" onClick={() => {
                                        const dto: ClientAnswersQuestionDto = {
                                            gameId: gameId,
                                            optionId: opt.optionId,
                                            eventType: StringConstants.ClientAnswersQuestionDto,
                                            questionId: currentQuestion.questionId,

                                        };
                                        send<ClientAnswersQuestionDto>(dto);

                                    }}>{opt.optionText}</button>

                                </div>
                            )}
                        </>
                        : gameState ? <div>{JSON.stringify(gameState)}</div>
                            :
                            <div>Waiting for question...</div>


                }

            </div>

        </>

    )
        ;
}