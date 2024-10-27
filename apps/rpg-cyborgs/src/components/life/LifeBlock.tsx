
import { Grid, GridItem, Heading, Stack } from '@chakra-ui/react'
import React from 'react'
import PointPanel from './PointPanel'
import { StatPanel } from '../stats'
import { useSelector } from 'react-redux'
import { selectDefence, selectLifePoints, selectMeleeAttack, selectRangedAttack, selectStaminaPoints } from '../../app/graphState/graphSelectors'

function LifeBlock() {
  const meleeAttack = useSelector(selectMeleeAttack)
  const rangedAttack = useSelector(selectRangedAttack)
  const defence = useSelector(selectDefence)
  const lifePoints = useSelector(selectLifePoints)
  const staminaPoints = useSelector(selectStaminaPoints)

  return (
    <Stack w={'100%'}>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        Combat
      </Heading>
      <Grid
        w={'100%'}
        gap={4}
        templateColumns={'repeat(6, 1fr)'}
        templateRows={'repeat(2, 1fr)'}
      >
        <GridItem colSpan={3} rowSpan={1}>
          <PointPanel
            name={'Stamina'}
            current={staminaPoints?.value ?? 0}
            max={staminaPoints?.baseValue ?? 0}
          />
        </GridItem>
        <GridItem colSpan={1} rowSpan={2}>
          <StatPanel
            propName={'Melee Attack'}
            propNameAbbr={''}
            propValue={meleeAttack}
          ></StatPanel>
        </GridItem>
        <GridItem colSpan={1} rowSpan={2}>
          <StatPanel
            propName={'Ranged Attack'}
            propNameAbbr={''}
            propValue={rangedAttack}
          ></StatPanel>
        </GridItem>
        <GridItem colSpan={1} rowSpan={2}>
          <StatPanel
            propName={'Defence'}
            propNameAbbr={''}
            propValue={defence}
          ></StatPanel>
        </GridItem>
        <GridItem colSpan={3} rowSpan={1}>
          <PointPanel
            name={'Life'}
            current={lifePoints?.value ?? 0}
            max={lifePoints?.baseValue ?? 0}
          />
        </GridItem>
      </Grid>
    </Stack>
  )
}

export default LifeBlock
