import { Heading, Stack, HStack } from '@chakra-ui/react'
import StatePanel from './StatePanel'
import { selectStates } from '../../app/states/statesSelectors'
import { useSelector } from 'react-redux'

function StatesBlock() {
  const states = useSelector(selectStates)
  return (
    <Stack w={'100%'}>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        States
      </Heading>
      <HStack w={'100%'} wrap={'wrap'} alignItems={'stretch'}>
        {states
          .filter((state) => state.isPlayerVisible)
          .map((state, i) => (
            <StatePanel key={i} state={state} />
          ))}
      </HStack>
    </Stack>
  )
}

export default StatesBlock
