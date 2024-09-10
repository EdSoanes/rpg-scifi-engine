import { useAtom } from 'jotai'
import { ButtonGroup, Heading, Stack } from '@chakra-ui/react'
import React from 'react'
import StateButton from './StateButton'
import { splitAtom } from 'jotai/utils'
import { playerCharacterStatesAtom } from '../atoms/playerCharacterStates.atom'

const stateAtomsAtom = splitAtom(playerCharacterStatesAtom)

function StatesBlock() {
  const [stateAtoms] = useAtom(stateAtomsAtom)
  return (
    <Stack w={'100%'}>
      <Heading as="h3" size="md" paddingBottom={4} paddingTop={10}>
        States
      </Heading>
      <ButtonGroup w={'100%'} alignItems={'stretch'}>
        {stateAtoms.map((state, i) => (
          <StateButton key={i} stateAtom={state} />
        ))}
      </ButtonGroup> 
    </Stack>
  )
}

export default StatesBlock
