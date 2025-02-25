import {useEffect, useState} from "react";
import {BaseDto, useWsClient} from "ws-request-hook";
import {
    ClientWantsToStartAGameDto,
    ServerAddsClientToGameDto,
    ServerSendsQuestionDto,
    StringConstants
} from "./generated-client.ts";
import ActiveGame from "./ActiveGame.tsx";

export default function Game() {

    const {onMessage, sendRequest, send} = useWsClient();
    const [gameId, setGameId] = useState<string | undefined>(undefined);


    useEffect(() => {
        const unsubscribe = onMessage<ServerSendsQuestionDto>(StringConstants.ServerSendsQuestionDto, (dto) => {
            console.log(dto);
        });
        unsubscribe();
    }, []);

    return (
        <>
        <div className="w-full h-full flex items-center justify-center flex-col">


            <button onClick={async () => {
                var dto: ClientWantsToStartAGameDto = {
                    eventType: StringConstants.ClientWantsToStartAGameDto
                }
                var result = await sendRequest<ClientWantsToStartAGameDto & BaseDto, ServerAddsClientToGameDto>(dto, StringConstants.ServerAddsClientToGameDto);
                setGameId(result.gameId!)
            }} className="btn btn-primary">Start game
            </button>


            <div>
                {

                    gameId ?
                        <ActiveGame gameid={gameId}/>
                        : null
                }
            </div>
        </div>         
     
</>

    );
}