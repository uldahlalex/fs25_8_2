import {useWsClient} from "ws-request-hook";
import {useEffect, useState} from "react";
import {
    ClientEntersLobbyDto,
    MemberHasLeftDto,
    ServerHasClientInLobbyDto,
    StringConstants
} from "./generated-client.ts";
import toast from "react-hot-toast";
import {send} from "vite";

export default function Lobby() {
    
    const {onMessage, sendRequest, send, readyState} = useWsClient();
    const [clients, setClients] = useState<string[]>([''])

    useEffect(() => {
        if (readyState != 1)
            return;
         onMessage<ServerHasClientInLobbyDto>(StringConstants.ServerHasClientInLobbyDto, (dto) => {
            setClients(dto.allClientIds!)
        })
        
        onMessage<MemberHasLeftDto>(StringConstants.MemberHasLeftDto, (dto) => {
            const duplicate =  [...clients]
            toast("Client "+dto.memberId+" has left!");
            setClients(duplicate.filter(c => c != dto.memberId))
        })
        const enterLobbyDto: ClientEntersLobbyDto = {
            eventType: StringConstants.ClientEntersLobbyDto,
        }
        send(enterLobbyDto);
        
      
    }, [readyState]);
    
    return (<>
        <div>Clients in the lobbby:</div>
        {
            clients.map(c => <div key={c}>{c}</div>)
        }

    </>)
}