import { useAtom } from 'jotai'
import { ButtonGroup } from '@chakra-ui/react'
import React from 'react'
import StateButton from './StateButton'
import { splitAtom } from 'jotai/utils'
import { playerCharacterStatesAtom } from '../atoms/playerCharacterStates.atom'

const stateAtomsAtom = splitAtom(playerCharacterStatesAtom)

function StatesBlock() {
  const [stateAtoms] = useAtom(stateAtomsAtom)
  return (
    <ButtonGroup w={'100%'} alignItems={'stretch'}>
      {stateAtoms.map((state, i) => (
        <StateButton key={i} stateAtom={state} />
      ))}
    </ButtonGroup>
  )
}

export default StatesBlock
