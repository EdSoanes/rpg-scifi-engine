import { Heading, Stack, Grid, GridItem } from '@chakra-ui/react'
import StatePanel from './StatePanel'
import { selectConditions } from '../../app/states/statesSelectors'
import { useSelector } from 'react-redux'
import { statePanel } from './StatesBlock.css'

function ConditionsBlock() {
  const conditions = useSelector(selectConditions)
  return (
    <Stack w={'100%'}>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        Conditions
      </Heading>
      <Grid templateColumns="repeat(6, 1fr)" gap="6">
        {conditions
          .filter((condition) => condition.isPlayerVisible)
          .map((condition, i) => (
            <GridItem key={i}>
              <StatePanel className={statePanel} state={condition} />
            </GridItem>
          ))}
      </Grid>
    </Stack>
  )
}

export default ConditionsBlock
