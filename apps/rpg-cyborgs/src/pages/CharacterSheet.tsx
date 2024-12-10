import { StatsBlock } from '@components/stats'
import { ConditionsBlock, StatesBlock } from '@components/states'
import { ActionTemplatesBlock } from '@components/activities'
import LifeBlock from '@components/life/LifeBlock'
import { GraphStateBlock } from '@components/graph'
import { useAppSelector } from '@app/hooks'
import { GearBlock } from '@components/gear'
import { Grid, GridItem } from '@chakra-ui/react'

export default function CharacterSheet() {
  const { hands, wearing } = useAppSelector((state) => state.gear)

  return (
    <Grid
      width={'100%'}
      maxW={{ xl: '1200px' }}
      templateRows="repeat(2, 1fr)"
      templateColumns="repeat(12, 1fr)"
      gap={6}
    >
      <GridItem colSpan={1} rowSpan={1}>
        <StatsBlock />
      </GridItem>
      <GridItem colSpan={2} rowSpan={1}>
        <ConditionsBlock />
      </GridItem>
      <GridItem colSpan={2} rowSpan={1}>
        <StatesBlock />
      </GridItem>
      <GridItem colSpan={7} rowSpan={1}>
        <ActionTemplatesBlock />
      </GridItem>
      <GridItem colSpan={12} rowSpan={1}>
        <LifeBlock />
      </GridItem>
      <GridItem colSpan={6}>
        <GearBlock name={'Hands'} container={hands} />
      </GridItem>
      <GridItem colSpan={6}>
        <GearBlock name={'Wearing'} container={wearing} />
      </GridItem>
      <GridItem colSpan={12}>
        <GraphStateBlock />
      </GridItem>
    </Grid>
  )
}
