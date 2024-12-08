import { HStack } from '@chakra-ui/react'
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
    <HStack w={'100%'} alignItems={'stretch'}>
      <StatPanel
        name="Strength"
        abbreviatedName="STR"
        prop={'Value'}
        propValue={strength}
      />
      <StatPanel
        name="Agility"
        abbreviatedName="AGI"
        prop={'Value'}
        propValue={agility}
      />
      <StatPanel
        name="Health"
        abbreviatedName="HEL"
        prop={'Value'}
        propValue={health}
      />
      <StatPanel
        name="Brains"
        abbreviatedName="BRA"
        prop={'Value'}
        propValue={brains}
      />
      <StatPanel
        name="Insight"
        abbreviatedName="INS"
        prop={'Value'}
        propValue={insight}
      />
      <StatPanel
        name="Charisma"
        abbreviatedName="CHA"
        prop={'Value'}
        propValue={charisma}
      />
    </HStack>
  )
}

export default StatsBlock
