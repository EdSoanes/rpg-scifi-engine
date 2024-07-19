import { atom, useAtom } from 'jotai'
import { Grid, GridItem, Heading, Stack } from '@chakra-ui/react'
import React from 'react'
import PointPanel from './PointPanel'
import { playerCharacterAtom } from '../atoms/playerCharacter.atom'
import { StatPanel } from '../stats'
import { PropValue } from '../../lib/rpg-api/types'

const meleeAttackAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.meleeAttack ?? null
)

const rangedAttackAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.rangedAttack ?? null
)

const defenceAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.defence ?? null
)
function LifeBlock() {
  const [playerCharacter] = useAtom(playerCharacterAtom)

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
            current={playerCharacter?.currentStaminaPoints ?? 0}
            max={playerCharacter?.staminaPoints ?? 0}
          />
        </GridItem>
        <GridItem colSpan={1} rowSpan={2}>
          <StatPanel
            propName={'Melee Attack'}
            propNameAbbr={''}
            propValueAtom={meleeAttackAtom}
          ></StatPanel>
        </GridItem>
        <GridItem colSpan={1} rowSpan={2}>
          <StatPanel
            propName={'Ranged Attack'}
            propNameAbbr={''}
            propValueAtom={rangedAttackAtom}
          ></StatPanel>
        </GridItem>
        <GridItem colSpan={1} rowSpan={2}>
          <StatPanel
            propName={'Defence'}
            propNameAbbr={''}
            propValueAtom={defenceAtom}
          ></StatPanel>
        </GridItem>
        <GridItem colSpan={3} rowSpan={1}>
          <PointPanel
            name={'Life'}
            current={playerCharacter?.currentLifePoints ?? 0}
            max={playerCharacter?.lifePoints ?? 0}
          />
        </GridItem>
      </Grid>
    </Stack>
  )
}

export default LifeBlock
