import { Grid, GridItem } from '@chakra-ui/react'
import StatPanel from './StatPanel'
import {
  selectAgility,
  selectBrains,
  selectCharisma,
  selectHealth,
  selectInsight,
  selectStrength,
} from '@app/graphState/graphSelectors'
import { useSelector } from 'react-redux'

function StatsBlock() {
  const strength = useSelector(selectStrength)
  const agility = useSelector(selectAgility)
  const health = useSelector(selectHealth)
  const brains = useSelector(selectBrains)
  const insight = useSelector(selectInsight)
  const charisma = useSelector(selectCharisma)

  return (
    <Grid templateColumns={'repeat(1, 1fr)'}>
      <GridItem>
        <StatPanel
          name="Strength"
          abbreviatedName="STR"
          prop={'Value'}
          propValue={strength}
        />
      </GridItem>
      <GridItem>
        <StatPanel
          name="Agility"
          abbreviatedName="AGI"
          prop={'Value'}
          propValue={agility}
        />
      </GridItem>
      <GridItem>
        <StatPanel
          name="Health"
          abbreviatedName="HEL"
          prop={'Value'}
          propValue={health}
        />
      </GridItem>
      <GridItem margin={0}>
        <StatPanel
          name="Brains"
          abbreviatedName="BRA"
          prop={'Value'}
          propValue={brains}
        />
      </GridItem>
      <GridItem>
        <StatPanel
          name="Insight"
          abbreviatedName="INS"
          prop={'Value'}
          propValue={insight}
        />
      </GridItem>
      <GridItem>
        <StatPanel
          name="Charisma"
          abbreviatedName="CHA"
          prop={'Value'}
          propValue={charisma}
        />
      </GridItem>
    </Grid>
  )
}

export default StatsBlock
