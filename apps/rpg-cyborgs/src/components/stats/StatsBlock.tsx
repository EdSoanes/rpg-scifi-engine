'use client'

import { StatGroup } from '@chakra-ui/react'
import React from 'react'
import StatPanel from './StatPanel'
import { selectAgility, selectBrains, selectCharisma, selectHealth, selectInsight, selectStrength } from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'

function StatsBlock() {
  const strength = useSelector(selectStrength)
  const agility = useSelector(selectAgility)
  const health = useSelector(selectHealth)
  const brains = useSelector(selectBrains)
  const insight = useSelector(selectInsight)
  const charisma = useSelector(selectCharisma)

  return (
    <StatGroup w={'100%'} alignItems={'stretch'}>
      <StatPanel
        propName="Strength"
        propNameAbbr="STR"
        propValue={strength}
      />
      <StatPanel
        propName="Agility"
        propNameAbbr="AGI"
        propValue={agility}
      />
      <StatPanel
        propName="Health"
        propNameAbbr="HEL"
        propValue={health}
      />
      <StatPanel
        propName="Brains"
        propNameAbbr="BRA"
        propValue={brains}
      />
      <StatPanel
        propName="Insight"
        propNameAbbr="INS"
        propValue={insight}
      />
      <StatPanel
        propName="Charisma"
        propNameAbbr="CHA"
        propValue={charisma}
      />
    </StatGroup>
  )
}

export default StatsBlock
