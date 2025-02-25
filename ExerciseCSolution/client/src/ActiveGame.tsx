import {useWsClient} from "ws-request-hook";
import {ClientWantsToGoToQuestionPhaseDto, ServerSendsQuestionDto, StringConstants} from "./generated-client.ts";
import {useEffect} from "react";

export interface ActiveGameProp {
    gameid: string
}
export default function ActiveGame(prop: ActiveGameProp) {

    const {onMessage, sendRequest, send} = useWsClient();

    useEffect(() => {
        const unsubscribe = onMessage<ServerSendsQuestionDto>(StringConstants.ServerSendsQuestionDto, (dto) => {
            console.log(dto);
        } )
    }, []);


    return(<>
        <div>game id: {prop.gameid}</div>
        <button onClick={async () => {
            var dto: ClientWantsToGoToQuestionPhaseDto = {
                gameId: prop.gameid,
                eventType: StringConstants.ClientWantsToGoToQuestionPhaseDto
            }
            send<ClientWantsToGoToQuestionPhaseDto>(dto);
        }}>Go to next question</button>
    </>)
} 