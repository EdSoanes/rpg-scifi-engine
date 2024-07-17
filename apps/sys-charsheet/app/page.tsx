'use client'

import { Button, ButtonGroup, Code } from '@chakra-ui/react'
import { Atom, PrimitiveAtom, useAtom, useAtomValue } from "jotai";
import { playerCharacterAtom, graphFetchAtom, graphStateAtom, stateAtomsAtom, actionAtomsAtom } from "@/lib/api/fetcher";
import { Action, State } from '@/lib/api/types';

const FetchGraphStateButton = () => {
  const [,fetchGraphState] = useAtom(graphFetchAtom)

  return (
    <div>
      <Button colorScheme='teal' variant='outline' onClick={fetchGraphState}>
        Get Benny
      </Button>
    </div>
  )
}

const StateButton = (
  { 
    stateAtom 
  } : {
    stateAtom: Atom<State>
  }
) => {
  const [state, setState] = useAtom(stateAtom)
  return (
    <Button colorScheme='green'>
      {state.name}
    </Button>
  )
}

const ActionButton = (
  { 
    actionAtom 
  } : {
    actionAtom: Atom<Action>
  }
) => {
  const [action, setAction] = useAtom(actionAtom)
  return (
    <Button colorScheme='blue'>
      {action.name}
    </Button>
  )
}

export default function Home() {
  const playerCharacter = useAtomValue(playerCharacterAtom)
  const [stateAtoms, stateDispatch] = useAtom(stateAtomsAtom)
  const [actionAtoms, actionDispatch] = useAtom(actionAtomsAtom)

  const graphState = useAtomValue(graphStateAtom)

  return (
    <div>
      <h1>{playerCharacter?.name ?? 'Nobody'}</h1>
      <FetchGraphStateButton/>
      {playerCharacter && (
        <div>
          <ButtonGroup gap='4'>
            {stateAtoms.map((state, i) =>
              <StateButton key={i} stateAtom={state}/>
            )}
          </ButtonGroup>          
          <ButtonGroup gap='4'>
            {actionAtoms.map((action, i) =>
              <ActionButton key={i} actionAtom={action}/>
            )}
          </ButtonGroup>

    
          <Code>
            {JSON.stringify(graphState, undefined, 2)}
          </Code>
        </div>)}
    </div>
  );
}
