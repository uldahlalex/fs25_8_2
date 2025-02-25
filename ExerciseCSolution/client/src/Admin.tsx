import {useState} from "react";
import {useWsClient} from "ws-request-hook";
import {
    AdminHasStartedGameDto,
    AdminWantsToStartGameDto, AdminWantsToStartQuestionDto,
    ServerAddsClientToGameDto,
    ServerConfirmsDto,
    StringConstants
} from "./generated-client.ts";
import toast from "react-hot-toast";

export default function Admin() {
    
    
    const [gameId, setGameId] = useState<string | undefined>();
    const [password, setPassword] = useState<string>('');
    
    const {onMessage, sendRequest, send,readyState}= useWsClient();
    
    const startGame= async () => {
        const dto: AdminWantsToStartGameDto = {
            password: password,
            eventType: StringConstants.AdminWantsToStartGameDto
        }
        const result = await sendRequest<AdminWantsToStartGameDto, AdminHasStartedGameDto>(dto, StringConstants.AdminHasStartedGameDto);
        setGameId(result.gameId);
        toast('Game has now begun!')
    }
    const nextQuestion = async () => {
        const dto: AdminWantsToStartQuestionDto = {
            password: password,
            eventType: StringConstants.AdminWantsToStartQuestionDto,
            gameId: gameId
        };
        var result = await sendRequest<AdminWantsToStartQuestionDto, ServerConfirmsDto>(dto,  StringConstants.ServerConfirmsDto);
        toast('Question is live!');
        
    }
    
    return(<>
        <div className="flex justify-center">Admin tools</div>
        
        <input className="input" placeholder="*****" type="password" value={password} onChange={e => setPassword(e.target.value)} />
        
        {
            gameId == undefined ?
                    <button className="btn btn-primary" onClick={startGame}>Start game!</button>   
                : <button className="btn btn-primary" onClick={nextQuestion}>Go to next question</button>
          

        }
    </>)
} 