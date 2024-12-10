import { Heading, HStack } from '@chakra-ui/react'
import { MeleeWeapon } from '@lib/rpg-api/cyborg-types'
import { GiBroadsword } from 'react-icons/gi'
import PropertyValue from '@components/prop/property-value'
import DescribePropertyDrawer from '@components/describe/DescribePropertyDrawer'

export declare interface MeleeWeaponProps {
  weapon: MeleeWeapon
}

function MeleeWeaponComponent(props: MeleeWeaponProps) {
  const { weapon } = props

  const onValueChanged = (value: number) => {
    console.log('Value', value)
  }

  return (
    <HStack>
      <GiBroadsword></GiBroadsword>
      <DescribePropertyDrawer entityId={weapon.id} prop={'HitBonus'}>
        <Heading>{weapon.name}</Heading>
      </DescribePropertyDrawer>
      <PropertyValue
        direction={'row'}
        entityId={weapon.id}
        prop={'HitBonus'}
        value={weapon.hitBonus}
        name={'Hit Bonus'}
        onPropValueChanged={onValueChanged}
      />
    </HStack>
  )
}

export default MeleeWeaponComponent
