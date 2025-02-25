import {useWsClient} from "ws-request-hook";
import {
    ClientWantsToGoToQuestionPhaseDto,
    Question,
    ServerSendsQuestionDto,
    StringConstants
} from "./generated-client.ts";
import {useEffect, useState} from "react";

export interface ActiveGameProp {
    gameid: string
}
export default function ActiveGame(prop: ActiveGameProp) {

    const {onMessage, sendRequest, send} = useWsClient();
    const [question, setQuestion] = useState<Question | undefined>();

    useEffect(() => {
        const unsubscribe = onMessage<ServerSendsQuestionDto>(StringConstants.ServerSendsQuestionDto, (dto) => {
            setQuestion(dto.question!)
        } )
    }, []);


    return(<div className="flex justify-center flex-col">
        <div>You have now joined game with ID:</div>
        <div> {prop.gameid}</div>
        <button className="btn btn-secondary" onClick={async () => {
            var dto: ClientWantsToGoToQuestionPhaseDto = {
                gameId: prop.gameid,
                eventType: StringConstants.ClientWantsToGoToQuestionPhaseDto
            }
            send<ClientWantsToGoToQuestionPhaseDto>(dto);
        }}>Click here to push question to all clients</button>
        {
            question ? (<>
                {question.questionText}
                {question.questionOptions?.map((option, index) => {
                    return <>Option {index+1}: {option.optionText} </>
                })}
            </>) : null
        }
    </div>)
} 