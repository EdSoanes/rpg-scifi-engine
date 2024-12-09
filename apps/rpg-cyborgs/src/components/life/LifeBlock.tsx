import { Grid, GridItem, Heading, Stack } from '@chakra-ui/react'
import PointPanel from './PointPanel'
import { StatPanel } from '../stats'
import { useSelector } from 'react-redux'
import {
  selectDefence,
  selectHead,
  selectLeftArm,
  selectLeftLeg,
  selectLifePoints,
  selectMeleeAttack,
  selectRangedAttack,
  selectRightArm,
  selectRightLeg,
  selectStaminaPoints,
  selectTorso,
} from '@app/graphState/graphSelectors'
import BodyPartPanel from './components/BodyPartPanel'

function LifeBlock() {
  const meleeAttack = useSelector(selectMeleeAttack)
  const rangedAttack = useSelector(selectRangedAttack)
  const defence = useSelector(selectDefence)
  const lifePoints = useSelector(selectLifePoints)
  const staminaPoints = useSelector(selectStaminaPoints)

  const head = useSelector(selectHead)
  const torso = useSelector(selectTorso)
  const leftArm = useSelector(selectLeftArm)
  const rightArm = useSelector(selectRightArm)
  const leftLeg = useSelector(selectLeftLeg)
  const rightLeg = useSelector(selectRightLeg)

  return (
    <>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        Combat
      </Heading>
      <Stack w={'100%'} direction={'row'}>
        <Grid w={'50%'}>
          <GridItem colSpan={3} rowSpan={1}>
            <PointPanel
              name={'Stamina'}
              current={staminaPoints?.value ?? 0}
              max={staminaPoints?.baseValue ?? 0}
            />
          </GridItem>
          <GridItem colSpan={3} rowSpan={1}>
            <PointPanel
              name={'Life'}
              current={lifePoints?.value ?? 0}
              max={lifePoints?.baseValue ?? 0}
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
        </Grid>
        <Grid w={'50%'}>
          <GridItem colSpan={3} rowSpan={1}>
            <BodyPartPanel bodyPart={head} />
          </GridItem>
          <GridItem colSpan={3} rowSpan={1}>
            <BodyPartPanel bodyPart={torso} />
          </GridItem>
          <GridItem colSpan={3} rowSpan={1}>
            <BodyPartPanel bodyPart={leftArm} />
          </GridItem>
          <GridItem colSpan={3} rowSpan={1}>
            <BodyPartPanel bodyPart={rightArm} />
          </GridItem>
          <GridItem colSpan={3} rowSpan={1}>
            <BodyPartPanel bodyPart={leftLeg} />
          </GridItem>
          <GridItem colSpan={3} rowSpan={1}>
            <BodyPartPanel bodyPart={rightLeg} />
          </GridItem>
        </Grid>
      </Stack>
    </>
  )
}

export default LifeBlock
