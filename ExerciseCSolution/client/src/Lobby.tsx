import {useWsClient} from "ws-request-hook";
import {useEffect} from "react";
import {
    ClientWantsToAuthenticateDto,
    ClientWantsToStartAGameDto,
    ServerAddsClientToGame,
    StringConstants
} from "./generated-client.ts";
import {useNavigate} from "react-router";

export default function Lobby() {
    
    // const {onMessage, sendRequest} = useWsClient();
    const navigate = useNavigate();

    useEffect(() => {
    
    }, []);
    
    return (
        <div>
            <h1>Lobby</h1>
            <button onClick={async() => {
                var dto: ClientWantsToStartAGameDto = {
                    
                }
                // var result = await sendRequest<ClientWantsToStartAGameDto, ServerAddsClientToGame>
                // (dto, StringConstants.ServerAddsClientToGame);
                // navigate('/game/'+result.gameId);
            }} className="btn btn-primary">Start new game from default question template</button>
        </div>
    )
} 