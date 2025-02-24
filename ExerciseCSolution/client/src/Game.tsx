import {useLocation, useParams} from "react-router";
import {useEffect} from "react";
import {useWsClient} from "ws-request-hook";
import {ServerSendsQuestionDto, StringConstants} from "./generated-client.ts";

export default function Game() {
    
    // const {onMessage, sendRequest} = useWsClient();
    
    // Get game ID from path /game/:gameid
    const {gameId} = useParams();
    console.log(gameId);

    useEffect(() => {
        // const unsubscribe = onMessage<ServerSendsQuestionDto>(StringConstants.ServerSendsQuestionDto, (dto) => {
        //     console.log(dto);
        // });
        // unsubscribe();
    }, []);
    
    return(<>
    
        
    
    </>);
}