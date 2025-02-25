import {useLocation, useParams} from "react-router";
import {useEffect, useState} from "react";
import {BaseDto, useWsClient} from "ws-request-hook";
import {
    ClientWantsToGoToQuestionPhaseDto,
    ClientWantsToStartAGameDto,
    ServerAddsClientToGameDto,
    ServerSendsQuestionDto,
    StringConstants
} from "./generated-client.ts";
import {send} from "vite";
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
    
    return(<>
    
        
            <button onClick={async() => {
                var dto: ClientWantsToStartAGameDto = {
                    eventType: StringConstants.ClientWantsToStartAGameDto
                }
                var result = await sendRequest<ClientWantsToStartAGameDto & BaseDto, ServerAddsClientToGameDto>(dto, StringConstants.ServerAddsClientToGameDto);
                setGameId(result.gameId!)
            }} className="btn btn-primary">Start game</button>
            {
                
                gameId ? 
                     <ActiveGame gameid={gameId} />
                 : null
            }
            
        </>
    
  
    );
}