import { Heading, Stack, Grid, GridItem } from '@chakra-ui/react'
import StatePanel from './StatePanel'
import { selectStates } from '@app/states/statesSelectors'
import { useSelector } from 'react-redux'
//import { statePanel } from './StatesBlock.css'

function StatesBlock() {
  const states = useSelector(selectStates)
  return (
    <Stack w={'100%'}>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        States
      </Heading>
      <Grid templateColumns="repeat(6, 1fr)" gap="6">
        {states
          .filter((state) => !!state.isPlayerVisible)
          .map((state, i) => (
            <GridItem key={i}>
              <StatePanel state={state} />
            </GridItem>
          ))}
      </Grid>
    </Stack>
  )
}

export default StatesBlock
