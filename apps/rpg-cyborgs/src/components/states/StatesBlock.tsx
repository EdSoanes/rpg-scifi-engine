import { ButtonGroup, Heading, Stack } from '@chakra-ui/react'
import React from 'react'
import StateButton from './StateButton'
import { selectStates } from '../../app/states/statesSelectors'
import { useSelector } from 'react-redux'

function StatesBlock() {
  const states = useSelector(selectStates)
  return (
    <Stack w={'100%'}>
      <Heading as="h3" size="md" paddingBottom={4} paddingTop={10}>
        States
      </Heading>
      <ButtonGroup w={'100%'} alignItems={'stretch'}>
        {states.map((state, i) => (
          <StateButton key={i} state={state} />
        ))}
      </ButtonGroup> 
    </Stack>
  )
}

export default StatesBlock
